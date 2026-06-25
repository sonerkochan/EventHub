using System.Security.Claims;
using EventHub.Areas.Organizer.Controllers;
using EventHub.Core.Contracts;
using EventHub.Core.Models.Refund;
using EventHub.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Moq;
using RefundStatus = EventHub.Infrastructure.Data.Models.Refund.RefundStatus;

namespace EventHub.Tests.Unit.Organizer;

[Trait("Category", "Unit")]
public class RefundsControllerTests
{
    [Fact]
    public async Task Index_CallsServiceWithOrganizerIdAndStatusFilter()
    {
        var organizerId = Guid.NewGuid();
        var refundService = new Mock<IRefundService>();
        var rows = new List<OrganizerRefundListItemViewModel>
        {
            new() { RefundId = Guid.NewGuid(), TicketId = Guid.NewGuid(), TicketNumber = 1001, EventId = Guid.NewGuid(), EventName = "Concert", BuyerId = Guid.NewGuid(), BuyerDisplay = "Buyer" }
        };
        refundService
            .Setup(s => s.GetRefundsForOrganizerAsync(organizerId, RefundStatus.Pending))
            .ReturnsAsync(rows);

        var controller = CreateController(refundService, organizerId);

        var result = await controller.Index(RefundStatus.Pending);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(rows, view.Model);
        Assert.Equal("Pending", (string?)controller.ViewBag.StatusFilter);
    }

    [Fact]
    public async Task Approve_WhenServiceSucceeds_SetsSuccessMessage()
    {
        var organizerId = Guid.NewGuid();
        var refundId = Guid.NewGuid();
        var refundService = new Mock<IRefundService>();
        refundService
            .Setup(s => s.ApproveTicketRefundAsync(refundId, organizerId))
            .ReturnsAsync(RefundOperationResult.Succeeded(refundId, 35f));

        var controller = CreateController(refundService, organizerId);

        var result = await controller.Approve(refundId);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Messages.Refund.Approved", controller.TempData["Success"]);
    }

    [Fact]
    public async Task Reject_WhenServiceFails_SetsErrorMessage()
    {
        var organizerId = Guid.NewGuid();
        var refundId = Guid.NewGuid();
        var refundService = new Mock<IRefundService>();
        refundService
            .Setup(s => s.RejectTicketRefundAsync(refundId, organizerId, "No"))
            .ReturnsAsync(RefundOperationResult.Failed("Messages.Refund.UnauthorizedOrganizer"));

        var controller = CreateController(refundService, organizerId);

        var result = await controller.Reject(refundId, "No");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Messages.Refund.UnauthorizedOrganizer", controller.TempData["Error"]);
    }

    private static RefundsController CreateController(Mock<IRefundService> refundService, Guid organizerId)
    {
        var localizer = new Mock<IStringLocalizer<MessagesResource>>();
        localizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, organizerId.ToString()) },
                "TestAuth"))
        };

        return new RefundsController(refundService.Object, localizer.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }
}
