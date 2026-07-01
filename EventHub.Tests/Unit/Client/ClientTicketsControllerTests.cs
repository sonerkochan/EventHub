using System.Security.Claims;
using EventHub.Areas.Client.Controllers;
using EventHub.Core.Contracts;
using EventHub.Core.Models.Refund;
using EventHub.Core.Models.Ticket;
using EventHub.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Moq;

namespace EventHub.Tests.Unit.Client;

[Trait("Category", "Unit")]
public class ClientTicketsControllerTests
{
    [Fact]
    public async Task RequestRefund_WhenServiceSucceeds_RedirectsToDetailsWithSuccessMessage()
    {
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ticketService = new Mock<ITicketService>();
        var refundService = new Mock<IRefundService>();
        refundService
            .Setup(s => s.RequestTicketRefundAsync(ticketId, userId, "Need to cancel"))
            .ReturnsAsync(RefundOperationResult.Succeeded(Guid.NewGuid(), 35f));

        var controller = CreateController(ticketService, refundService, userId);

        var result = await controller.RequestRefund(ticketId, "Need to cancel");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal(ticketId, redirect.RouteValues?["id"]);
        Assert.Equal("Messages.Refund.Requested", controller.TempData["Success"]);
    }

    [Fact]
    public async Task RequestRefund_WhenServiceFails_RedirectsToDetailsWithErrorMessage()
    {
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ticketService = new Mock<ITicketService>();
        var refundService = new Mock<IRefundService>();
        refundService
            .Setup(s => s.RequestTicketRefundAsync(ticketId, userId, null))
            .ReturnsAsync(RefundOperationResult.Failed("Messages.Refund.TooLate"));

        var controller = CreateController(ticketService, refundService, userId);

        var result = await controller.RequestRefund(ticketId, null);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Messages.Refund.TooLate", controller.TempData["Error"]);
    }

    [Fact]
    public async Task Index_LoadsUserTicketsIntoView()
    {
        var userId = Guid.NewGuid();
        var ticketService = new Mock<ITicketService>();
        var refundService = new Mock<IRefundService>();
        var tickets = new List<TicketListViewModel> { new() { Id = Guid.NewGuid(), EventName = "Concert", RoomName = "Hall" } };
        ticketService.Setup(s => s.GetUserTicketsAsync(userId)).ReturnsAsync(tickets);

        var controller = CreateController(ticketService, refundService, userId);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(tickets, view.Model);
    }

    private static TicketsController CreateController(
        Mock<ITicketService> ticketService,
        Mock<IRefundService> refundService,
        Guid userId)
    {
        var localizer = new Mock<IStringLocalizer<MessagesResource>>();
        localizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                "TestAuth"))
        };

        return new TicketsController(ticketService.Object, refundService.Object, localizer.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }
}
