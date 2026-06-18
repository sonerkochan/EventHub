using System.Security.Claims;
using EventHub.Core.Contracts;
using EventHub.Core.Models.ApplicationForm;
using EventHub.Infrastructure.Data.Models;
using EventHub.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Moq;
using ApplicationController = EventHub.Areas.Admin.Controllers.ApplicationController;

namespace EventHub.Tests.Unit.Admin;

public class AdminApplicationControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithPendingApplications()
    {
        var pendingApplications = new List<ApplicationListViewModel>
        {
            new()
            {
                Id = 1,
                UserName = "user organizer",
                Type = ApplicationType.Organizer,
                Description = "Organizer application",
                OrganizationName = "Event Org",
                PhoneNumber = "123456",
                CreatedAt = new DateTime(2026, 5, 23)
            }
        };
        var applicationService = new Mock<IApplicationService>();
        applicationService
            .Setup(s => s.GetAllPendingAsync())
            .ReturnsAsync(pendingApplications);
        var controller = new ApplicationController(applicationService.Object, CreateMessagesLocalizer().Object);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<ApplicationListViewModel>>(viewResult.Model);
        Assert.Same(pendingApplications, model);
        applicationService.Verify(s => s.GetAllPendingAsync(), Times.Once);
    }

    [Fact]
    public async Task Approve_CallsApproveAsync_WithCorrectApplicationIdAndAdminId()
    {
        const int applicationId = 7;
        const string adminId = "admin-user-id";
        var applicationService = new Mock<IApplicationService>();
        applicationService
            .Setup(s => s.ApproveAsync(applicationId, adminId))
            .ReturnsAsync(true);
        var controller = CreateController(applicationService, adminId);

        var result = await controller.Approve(applicationId);

        applicationService.Verify(s => s.ApproveAsync(applicationId, adminId), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ApplicationController.Index), redirect.ActionName);
        Assert.Equal("Application approved.", controller.TempData["Success"]);
    }

    [Fact]
    public async Task Reject_CallsRejectAsync_WithCorrectParameters()
    {
        const int applicationId = 7;
        const string adminId = "admin-user-id";
        const string comment = "Missing required information.";
        var applicationService = new Mock<IApplicationService>();
        applicationService
            .Setup(s => s.RejectAsync(applicationId, adminId, comment))
            .ReturnsAsync(true);
        var controller = CreateController(applicationService, adminId);

        var result = await controller.Reject(applicationId, comment);

        applicationService.Verify(s => s.RejectAsync(applicationId, adminId, comment), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ApplicationController.Index), redirect.ActionName);
        Assert.Equal("Application rejected.", controller.TempData["Success"]);
    }

    private static ApplicationController CreateController(
        Mock<IApplicationService> applicationService,
        string adminId)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, adminId) },
                "TestAuth"))
        };

        var controller = new ApplicationController(applicationService.Object, CreateMessagesLocalizer().Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new Mock<ITempDataProvider>().Object)
        };

        return controller;
    }

    private static Mock<IStringLocalizer<MessagesResource>> CreateMessagesLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<MessagesResource>>();
        localizer
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key switch
            {
                "Messages.Application.Approved" => "Application approved.",
                "Messages.Application.Rejected" => "Application rejected.",
                _ => key
            }));

        return localizer;
    }
}
