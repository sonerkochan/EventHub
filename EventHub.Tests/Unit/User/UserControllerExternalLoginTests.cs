using System.Security.Claims;
using EventHub.Controllers;
using EventHub.Core.Contracts;
using EventHub.Core.Models.User;
using EventHub.Localization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EventHub.Tests.Unit.Authentication;

public class UserControllerExternalLoginTests
{
    [Fact]
    public void ExternalLogin_WithGoogleProvider_ReturnsChallenge()
    {
        var signInManager = CreateSignInManagerMock();
        var properties = new AuthenticationProperties { RedirectUri = "/callback" };
        signInManager
            .Setup(s => s.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme,
                "/callback",
                null))
            .Returns(properties);
        var controller = CreateController(signInManager: signInManager);
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
            .Returns("/callback");
        controller.Url = urlHelper.Object;

        var result = controller.ExternalLogin(GoogleDefaults.AuthenticationScheme);

        var challenge = Assert.IsType<ChallengeResult>(result);
        Assert.Same(properties, challenge.Properties);
        Assert.Contains(GoogleDefaults.AuthenticationScheme, challenge.AuthenticationSchemes);
    }

    [Fact]
    public async Task ExternalLoginCallback_WhenExternalUserSucceeds_RedirectsToRoleArea()
    {
        var user = new EventHub.Infrastructure.Data.Models.User
        {
            Id = "user-id",
            UserName = "client-user",
            Email = "client@example.com",
            IsActive = true
        };
        var loginInfo = CreateLoginInfo("client@example.com", "google-client");
        var userManager = CreateUserManagerMock();
        userManager
            .Setup(u => u.GetRolesAsync(user))
            .ReturnsAsync(["Client"]);
        var signInManager = CreateSignInManagerMock(userManager);
        signInManager
            .Setup(s => s.GetExternalLoginInfoAsync(null))
            .ReturnsAsync(loginInfo);
        signInManager
            .Setup(s => s.SignInAsync(user, false, "Google"))
            .Returns(Task.CompletedTask);
        var externalAuth = new Mock<IExternalAuthService>();
        externalAuth
            .Setup(s => s.HandleExternalLoginCallbackAsync(loginInfo, It.IsAny<string?>(), It.IsAny<string>()))
            .ReturnsAsync(ExternalLoginProcessResult.Success(user));
        var controller = CreateController(userManager, signInManager, externalAuth);

        var result = await controller.ExternalLoginCallback();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
        Assert.Equal("Client", redirect.RouteValues?["area"]);
        signInManager.Verify(s => s.SignInAsync(user, false, "Google"), Times.Once);
    }

    [Fact]
    public async Task ExternalLoginCallback_WhenNewExternalUser_RedirectsToConfirmation()
    {
        var loginInfo = CreateLoginInfo("new@example.com", "google-new");
        var signInManager = CreateSignInManagerMock();
        signInManager
            .Setup(s => s.GetExternalLoginInfoAsync(null))
            .ReturnsAsync(loginInfo);
        var externalAuth = new Mock<IExternalAuthService>();
        externalAuth
            .Setup(s => s.HandleExternalLoginCallbackAsync(loginInfo, It.IsAny<string?>(), It.IsAny<string>()))
            .ReturnsAsync(ExternalLoginProcessResult.RequiresConfirmation("new@example.com", "Google"));
        var controller = CreateController(signInManager: signInManager, externalAuthService: externalAuth);

        var result = await controller.ExternalLoginCallback();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ExternalLoginConfirmation", redirect.ActionName);
    }

    [Fact]
    public async Task ExternalLoginConfirmation_WhenLoginInfoExists_ReturnsViewWithModel()
    {
        var loginInfo = CreateLoginInfo("new@example.com", "google-new");
        var signInManager = CreateSignInManagerMock();
        signInManager
            .Setup(s => s.GetExternalLoginInfoAsync(null))
            .ReturnsAsync(loginInfo);
        var controller = CreateController(signInManager: signInManager);

        var result = await controller.ExternalLoginConfirmation();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ExternalLoginConfirmationViewModel>(view.Model);
        Assert.Equal("new@example.com", model.Email);
        Assert.Equal("Google", model.Provider);
    }

    private static UserController CreateController(
        Mock<UserManager<EventHub.Infrastructure.Data.Models.User>>? userManager = null,
        Mock<SignInManager<EventHub.Infrastructure.Data.Models.User>>? signInManager = null,
        Mock<IExternalAuthService>? externalAuthService = null)
    {
        userManager ??= CreateUserManagerMock();
        signInManager ??= CreateSignInManagerMock(userManager);
        externalAuthService ??= new Mock<IExternalAuthService>();
        var roleManager = CreateRoleManagerMock();
        var localizer = new Mock<IStringLocalizer<MessagesResource>>();
        localizer
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        var controller = new UserController(
            userManager.Object,
            signInManager.Object,
            roleManager.Object,
            externalAuthService.Object,
            localizer.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return controller;
    }

    private static Mock<UserManager<EventHub.Infrastructure.Data.Models.User>> CreateUserManagerMock()
        => new(
            Mock.Of<IUserStore<EventHub.Infrastructure.Data.Models.User>>(),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);

    private static Mock<SignInManager<EventHub.Infrastructure.Data.Models.User>> CreateSignInManagerMock(
        Mock<UserManager<EventHub.Infrastructure.Data.Models.User>>? userManager = null)
    {
        userManager ??= CreateUserManagerMock();

        return new Mock<SignInManager<EventHub.Infrastructure.Data.Models.User>>(
            userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<EventHub.Infrastructure.Data.Models.User>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<ILogger<SignInManager<EventHub.Infrastructure.Data.Models.User>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<EventHub.Infrastructure.Data.Models.User>>());
    }

    private static Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
        => new(
            Mock.Of<IRoleStore<IdentityRole>>(),
            null,
            null,
            null,
            null);

    private static ExternalLoginInfo CreateLoginInfo(string email, string providerKey)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new("email_verified", "true")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Google"));

        return new ExternalLoginInfo(principal, "Google", providerKey, "Google");
    }
}
