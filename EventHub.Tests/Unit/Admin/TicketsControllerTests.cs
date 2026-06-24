using System.Security.Claims;
using EventHub.Areas.Admin.Controllers;
using EventHub.Core.Contracts;
using EventHub.Core.Models.Admin;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventHub.Tests.Unit.Admin;

[Trait("Category", "Unit")]
public class TicketsControllerTests
{
    [Fact]
    public async Task Index_WithValidStatusFilter_ReturnsViewWithFilteredRowsAndViewBagValues()
    {
        var rows = new List<AdminTicketRow>
    {
        CreateTicketRow(ticketNumber: 1002, status: TicketStatus.Purchased, eventName: "Concert B"),
        CreateTicketRow(ticketNumber: 1001, status: TicketStatus.Purchased, eventName: "Concert A")
    };

        var ticketService = new Mock<ITicketService>();
        ticketService
            .Setup(s => s.GetAllForAdminAsync(TicketStatus.Purchased))
            .ReturnsAsync(rows);

        var controller = new TicketsController(ticketService.Object);

        var result = await controller.Index(status: "Purchased", sort: "number", dir: "asc");

        var viewResult = Assert.IsType<ViewResult>(result);

        var model = Assert
            .IsAssignableFrom<IEnumerable<AdminTicketRow>>(viewResult.Model)
            .ToList();

        var modelTicketNumbers = model
            .Select(t => t.TicketNumber)
            .ToList();

        Assert.Equal(new long[] { 1001, 1002 }, modelTicketNumbers);

        Assert.Equal("Purchased", (string?)controller.ViewBag.StatusFilter);
        Assert.Equal("number", (string?)controller.ViewBag.Sort);
        Assert.Equal("asc", (string?)controller.ViewBag.Dir);
        Assert.Equal(1, (int)controller.ViewBag.Page);
        Assert.Equal(10, (int)controller.ViewBag.PageSize);
        Assert.Equal(2, (int)controller.ViewBag.TotalCount);

        object allRowsObject = controller.ViewBag.AllRows;

        var allRows = Assert
            .IsAssignableFrom<IEnumerable<AdminTicketRow>>(allRowsObject)
            .ToList();

        var allRowTicketNumbers = allRows
            .Select(t => t.TicketNumber)
            .ToList();

        Assert.Equal(new long[] { 1002, 1001 }, allRowTicketNumbers);

        ticketService.Verify(
            s => s.GetAllForAdminAsync(TicketStatus.Purchased),
            Times.Once);
    }

    [Fact]
    public async Task Index_WithInvalidStatusFilter_CallsServiceWithoutStatusFilter()
    {
        var rows = new List<AdminTicketRow>
        {
            CreateTicketRow(ticketNumber: 1001, status: TicketStatus.Purchased)
        };
        var ticketService = new Mock<ITicketService>();
        ticketService
            .Setup(s => s.GetAllForAdminAsync(null))
            .ReturnsAsync(rows);
        var controller = new TicketsController(ticketService.Object);

        var result = await controller.Index(status: "NotAStatus");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<AdminTicketRow>>(viewResult.Model).ToList();
        Assert.Single(model);
        Assert.Null(controller.ViewBag.StatusFilter);
        ticketService.Verify(s => s.GetAllForAdminAsync(null), Times.Once);
    }

    [Fact]
    public async Task Index_WithPaging_ReturnsRequestedPageItems()
    {
        var rows = Enumerable.Range(1, 25)
            .Select(i => CreateTicketRow(ticketNumber: 1000 + i, status: TicketStatus.Purchased))
            .ToList();
        var ticketService = new Mock<ITicketService>();
        ticketService
            .Setup(s => s.GetAllForAdminAsync(null))
            .ReturnsAsync(rows);
        var controller = new TicketsController(ticketService.Object);

        var result = await controller.Index(sort: "number", dir: "asc", page: 2, size: 10);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<AdminTicketRow>>(viewResult.Model).ToList();
        Assert.Equal(10, model.Count);
        Assert.Equal(1011, model[0].TicketNumber);
        Assert.Equal(1020, model[^1].TicketNumber);
        Assert.Equal(2, controller.ViewBag.Page);
        Assert.Equal(10, controller.ViewBag.PageSize);
        Assert.Equal(3, controller.ViewBag.TotalPages);
        Assert.Equal(25, controller.ViewBag.TotalCount);
    }

    [Fact]
    public async Task LookupPartial_WithBlankQuery_ReturnsLookupModalWithNullModelAndDoesNotCallService()
    {
        var ticketService = new Mock<ITicketService>();
        var controller = new TicketsController(ticketService.Object);

        var result = await controller.LookupPartial(" ");

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_LookupModal", partial.ViewName);
        Assert.Null(partial.Model);
        ticketService.Verify(s => s.LookupAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LookupPartial_WithQuery_CallsLookupAsyncAndReturnsLookupModal()
    {
        const string query = " 1001 ";
        var lookup = new AdminTicketLookupDto
        {
            Id = Guid.NewGuid(),
            TicketNumber = 1001,
            EventName = "Concert",
            Status = TicketStatus.Purchased
        };
        var ticketService = new Mock<ITicketService>();
        ticketService
            .Setup(s => s.LookupAsync(query))
            .ReturnsAsync(lookup);
        var controller = new TicketsController(ticketService.Object);

        var result = await controller.LookupPartial(query);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_LookupModal", partial.ViewName);
        Assert.Same(lookup, partial.Model);
        Assert.Equal("1001", controller.ViewBag.SearchedQuery);
        ticketService.Verify(s => s.LookupAsync(query), Times.Once);
    }

    [Fact]
    public async Task Refund_CallsAdminRefundTicketAsyncWithTicketIdAndAdminId_ReturnsJsonResult()
    {
        var ticketId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var ticketService = new Mock<ITicketService>();
        ticketService
            .Setup(s => s.AdminRefundTicketAsync(ticketId, adminId))
            .ReturnsAsync(true);
        var controller = CreateController(ticketService, adminId);

        var result = await controller.Refund(ticketId);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        ticketService.Verify(s => s.AdminRefundTicketAsync(ticketId, adminId), Times.Once);
    }

    [Fact]
    public async Task EditPartial_WhenTicketExists_ReturnsEditModalPartialWithModel()
    {
        var ticketId = Guid.NewGuid();
        var model = new AdminTicketEditViewModel
        {
            TicketId = ticketId,
            TicketNumber = 1001,
            EventName = "Concert",
            CurrentStatus = TicketStatus.Purchased
        };
        var ticketService = new Mock<ITicketService>();
        ticketService
            .Setup(s => s.GetForAdminEditAsync(ticketId))
            .ReturnsAsync(model);
        var controller = new TicketsController(ticketService.Object);

        var result = await controller.EditPartial(ticketId);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_EditModal", partial.ViewName);
        Assert.Same(model, partial.Model);
        ticketService.Verify(s => s.GetForAdminEditAsync(ticketId), Times.Once);
    }

    [Fact]
    public async Task EditPartial_WhenTicketDoesNotExist_ReturnsNotFound()
    {
        var ticketId = Guid.NewGuid();
        var ticketService = new Mock<ITicketService>();
        ticketService
            .Setup(s => s.GetForAdminEditAsync(ticketId))
            .ReturnsAsync((AdminTicketEditViewModel?)null);
        var controller = new TicketsController(ticketService.Object);

        var result = await controller.EditPartial(ticketId);

        Assert.IsType<NotFoundResult>(result);
        ticketService.Verify(s => s.GetForAdminEditAsync(ticketId), Times.Once);
    }

    [Fact]
    public async Task Edit_CallsAdminUpdateTicketAsync_ReturnsJsonResult()
    {
        var request = new AdminTicketEditRequest
        {
            TicketId = Guid.NewGuid(),
            SeatId = Guid.NewGuid(),
            Status = TicketStatus.Used
        };
        var ticketService = new Mock<ITicketService>();
        ticketService
            .Setup(s => s.AdminUpdateTicketAsync(request))
            .ReturnsAsync((true, "Updated"));
        var controller = new TicketsController(ticketService.Object);

        var result = await controller.Edit(request);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        Assert.Equal("Updated", ReadMessageProperty(json.Value));
        ticketService.Verify(s => s.AdminUpdateTicketAsync(request), Times.Once);
    }

    private static TicketsController CreateController(Mock<ITicketService> ticketService, Guid adminId)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, adminId.ToString()) },
                "TestAuth"))
        };

        return new TicketsController(ticketService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static AdminTicketRow CreateTicketRow(
        long ticketNumber,
        TicketStatus status,
        string eventName = "Concert")
        => new()
        {
            Id = Guid.NewGuid(),
            TicketNumber = ticketNumber,
            EventId = Guid.NewGuid(),
            EventName = eventName,
            EventStart = new DateTime(2026, 6, 1).AddDays(ticketNumber),
            SeatId = Guid.NewGuid(),
            SeatNumber = (int)(ticketNumber % 100),
            Status = status,
            Price = 25,
            Currency = "EUR",
            BuyerUserId = Guid.NewGuid(),
            BuyerDisplay = $"Buyer {ticketNumber}",
            ReservedAt = new DateTime(2026, 5, 1).AddMinutes(ticketNumber),
            PurchasedAt = new DateTime(2026, 5, 2).AddMinutes(ticketNumber)
        };

    private static bool ReadSuccessProperty(object? value)
    {
        Assert.NotNull(value);
        var property = value.GetType().GetProperty("success");
        Assert.NotNull(property);
        return Assert.IsType<bool>(property.GetValue(value));
    }

    private static string? ReadMessageProperty(object? value)
    {
        Assert.NotNull(value);
        var property = value.GetType().GetProperty("message");
        Assert.NotNull(property);
        return property.GetValue(value) as string;
    }
}
