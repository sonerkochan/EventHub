using EventHub.Areas.Admin.Controllers;
using EventHub.Core.Contracts;
using EventHub.Core.Models.Moderator;
using EventHub.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;

namespace EventHub.Tests.Unit.Admin;

[Trait("Category", "Unit")]
public class ModeratorControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithModerators()
    {
        var moderators = new List<ModeratorListViewModel>
        {
            new()
            {
                Id = "moderator-id",
                Username = "moderator",
                Email = "moderator@example.com",
                FirstName = "Test",
                LastName = "Moderator",
                IsActive = true,
                CreatedAt = new DateTime(2026, 5, 24)
            }
        };
        var moderatorService = new Mock<IModeratorService>();
        moderatorService
            .Setup(s => s.GetAllModeratorsAsync())
            .ReturnsAsync(moderators);
        var controller = new ModeratorController(moderatorService.Object, CreateMessagesLocalizer().Object);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<ModeratorListViewModel>>(viewResult.Model);
        Assert.Same(moderators, model);
        moderatorService.Verify(s => s.GetAllModeratorsAsync(), Times.Once);
    }

    [Fact]
    public void CreateModeratorGet_ReturnsViewWithAddModeratorViewModel()
    {
        var moderatorService = new Mock<IModeratorService>();
        var controller = new ModeratorController(moderatorService.Object, CreateMessagesLocalizer().Object);

        var result = controller.Create();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<AddModeratorViewModel>(viewResult.Model);
        moderatorService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateModeratorPost_WhenModelStateIsInvalid_ReturnsViewAndDoesNotCallService()
    {
        var moderatorService = new Mock<IModeratorService>();
        var controller = new ModeratorController(moderatorService.Object, CreateMessagesLocalizer().Object);
        var model = CreateAddModel();
        controller.ModelState.AddModelError("Email", "Required");

        var result = await controller.Create(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        moderatorService.Verify(s => s.CreateModeratorAsync(It.IsAny<AddModeratorViewModel>()), Times.Never);
    }

    [Fact]
    public async Task CreateModeratorPost_WhenServiceSucceeds_RedirectsToIndex()
    {
        var model = CreateAddModel();
        var moderatorService = new Mock<IModeratorService>();
        moderatorService
            .Setup(s => s.CreateModeratorAsync(model))
            .ReturnsAsync(true);
        var controller = new ModeratorController(moderatorService.Object, CreateMessagesLocalizer().Object);

        var result = await controller.Create(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ModeratorController.Index), redirect.ActionName);
        moderatorService.Verify(s => s.CreateModeratorAsync(model), Times.Once);
    }

    [Fact]
    public async Task CreateModeratorPost_WhenServiceFails_ReturnsViewWithModelError()
    {
        var model = CreateAddModel();
        var moderatorService = new Mock<IModeratorService>();
        moderatorService
            .Setup(s => s.CreateModeratorAsync(model))
            .ReturnsAsync(false);
        var controller = new ModeratorController(moderatorService.Object, CreateMessagesLocalizer().Object);

        var result = await controller.Create(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(
            controller.ModelState[string.Empty]!.Errors,
            e => e.ErrorMessage == "Failed to create moderator. Username or email may already be taken.");
        moderatorService.Verify(s => s.CreateModeratorAsync(model), Times.Once);
    }

    [Fact]
    public async Task EditModeratorGet_WhenModeratorExists_ReturnsViewWithModel()
    {
        const string id = "moderator-id";
        var model = CreateEditModel(id);
        var moderatorService = new Mock<IModeratorService>();
        moderatorService
            .Setup(s => s.GetModeratorByIdAsync(id))
            .ReturnsAsync(model);
        var controller = new ModeratorController(moderatorService.Object, CreateMessagesLocalizer().Object);

        var result = await controller.Edit(id);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        moderatorService.Verify(s => s.GetModeratorByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task EditModeratorGet_WhenModeratorDoesNotExist_ReturnsNotFound()
    {
        const string id = "missing-id";
        var moderatorService = new Mock<IModeratorService>();
        moderatorService
            .Setup(s => s.GetModeratorByIdAsync(id))
            .ReturnsAsync((EditModeratorViewModel?)null);
        var controller = new ModeratorController(moderatorService.Object, CreateMessagesLocalizer().Object);

        var result = await controller.Edit(id);

        Assert.IsType<NotFoundResult>(result);
        moderatorService.Verify(s => s.GetModeratorByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task EditModeratorPost_WhenModelStateIsInvalid_ReturnsViewAndDoesNotCallService()
    {
        var model = CreateEditModel("moderator-id");
        var moderatorService = new Mock<IModeratorService>();
        var controller = new ModeratorController(moderatorService.Object, CreateMessagesLocalizer().Object);
        controller.ModelState.AddModelError("Username", "Required");

        var result = await controller.Edit(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        moderatorService.Verify(s => s.EditModeratorAsync(It.IsAny<EditModeratorViewModel>()), Times.Never);
    }

    [Fact]
    public async Task EditModeratorPost_WhenServiceSucceeds_RedirectsToIndex()
    {
        var model = CreateEditModel("moderator-id");
        var moderatorService = new Mock<IModeratorService>();
        moderatorService
            .Setup(s => s.EditModeratorAsync(model))
            .ReturnsAsync(true);
        var controller = new ModeratorController(moderatorService.Object, CreateMessagesLocalizer().Object);

        var result = await controller.Edit(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ModeratorController.Index), redirect.ActionName);
        moderatorService.Verify(s => s.EditModeratorAsync(model), Times.Once);
    }

    [Fact]
    public async Task EditModeratorPost_WhenServiceFails_ReturnsViewWithModelError()
    {
        var model = CreateEditModel("moderator-id");
        var moderatorService = new Mock<IModeratorService>();
        moderatorService
            .Setup(s => s.EditModeratorAsync(model))
            .ReturnsAsync(false);
        var controller = new ModeratorController(moderatorService.Object, CreateMessagesLocalizer().Object);

        var result = await controller.Edit(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(
            controller.ModelState[string.Empty]!.Errors,
            e => e.ErrorMessage == "Failed to update moderator.");
        moderatorService.Verify(s => s.EditModeratorAsync(model), Times.Once);
    }

    [Fact]
    public async Task DisableModerator_CallsSetActiveStatusAsyncWithFalseAndRedirectsToIndex()
    {
        const string id = "moderator-id";
        var moderatorService = new Mock<IModeratorService>();
        moderatorService
            .Setup(s => s.SetActiveStatusAsync(id, false))
            .ReturnsAsync(true);
        var controller = new ModeratorController(moderatorService.Object, CreateMessagesLocalizer().Object);

        var result = await controller.Disable(id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ModeratorController.Index), redirect.ActionName);
        moderatorService.Verify(s => s.SetActiveStatusAsync(id, false), Times.Once);
    }

    [Fact]
    public async Task EnableModerator_CallsSetActiveStatusAsyncWithTrueAndRedirectsToIndex()
    {
        const string id = "moderator-id";
        var moderatorService = new Mock<IModeratorService>();
        moderatorService
            .Setup(s => s.SetActiveStatusAsync(id, true))
            .ReturnsAsync(true);
        var controller = new ModeratorController(moderatorService.Object, CreateMessagesLocalizer().Object);

        var result = await controller.Enable(id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ModeratorController.Index), redirect.ActionName);
        moderatorService.Verify(s => s.SetActiveStatusAsync(id, true), Times.Once);
    }

    private static Mock<IStringLocalizer<MessagesResource>> CreateMessagesLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<MessagesResource>>();
        localizer
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key switch
            {
                "Messages.Moderator.CreateFailed" => "Failed to create moderator. Username or email may already be taken.",
                "Messages.Moderator.UpdateFailed" => "Failed to update moderator.",
                _ => key
            }));

        return localizer;
    }

    private static AddModeratorViewModel CreateAddModel()
        => new()
        {
            Username = "moderator",
            Email = "moderator@test.com",
            FirstName = "Test",
            LastName = "Moderator",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

    private static EditModeratorViewModel CreateEditModel(string id)
        => new()
        {
            Id = id,
            Username = "moderator",
            Email = "moderator@example.com",
            FirstName = "Test",
            LastName = "Moderator"
        };
}
