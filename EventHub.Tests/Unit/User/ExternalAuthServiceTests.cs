using System.Security.Claims;
using EventHub.Core.Contracts;
using EventHub.Core.Models.User;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventHub.Tests.Unit.Authentication;

public class ExternalAuthServiceTests
{
    [Fact]
    public async Task HandleExternalLoginCallback_WhenLoginAlreadyLinkedAndUserActive_Succeeds()
    {
        using var services = await CreateServicesAsync();
        var userManager = services.GetRequiredService<UserManager<EventHub.Infrastructure.Data.Models.User>>();
        var service = services.GetRequiredService<IExternalAuthService>();
        var user = await CreateUserAsync(userManager, "linked-user", "linked@example.com");
        var loginInfo = CreateLoginInfo("linked@example.com", "google-linked");
        await userManager.AddLoginAsync(user, loginInfo);

        var result = await service.HandleExternalLoginCallbackAsync(loginInfo, "127.0.0.1", "test-device");

        Assert.True(result.Succeeded);
        Assert.Equal(user.Id, result.User?.Id);
    }

    [Fact]
    public async Task HandleExternalLoginCallback_WhenLinkedUserInactive_IsRejected()
    {
        using var services = await CreateServicesAsync();
        var userManager = services.GetRequiredService<UserManager<EventHub.Infrastructure.Data.Models.User>>();
        var service = services.GetRequiredService<IExternalAuthService>();
        var user = await CreateUserAsync(userManager, "inactive-user", "inactive@example.com", isActive: false);
        var loginInfo = CreateLoginInfo("inactive@example.com", "google-inactive");
        await userManager.AddLoginAsync(user, loginInfo);

        var result = await service.HandleExternalLoginCallbackAsync(loginInfo, null, "test-device");

        Assert.Equal(ExternalLoginProcessStatus.AccountInactive, result.Status);
    }

    [Fact]
    public async Task HandleExternalLoginCallback_WhenVerifiedEmailMatchesExistingUser_AutoLinksAndSucceeds()
    {
        using var services = await CreateServicesAsync();
        var userManager = services.GetRequiredService<UserManager<EventHub.Infrastructure.Data.Models.User>>();
        var service = services.GetRequiredService<IExternalAuthService>();
        var user = await CreateUserAsync(userManager, "existing-user", "existing@example.com");
        var loginInfo = CreateLoginInfo("existing@example.com", "google-existing");

        var result = await service.HandleExternalLoginCallbackAsync(loginInfo, "127.0.0.1", "test-device");
        var linkedUser = await userManager.FindByLoginAsync("Google", "google-existing");

        Assert.True(result.Succeeded);
        Assert.Equal(user.Id, result.User?.Id);
        Assert.Equal(user.Id, linkedUser?.Id);
    }

    [Fact]
    public async Task HandleExternalLoginCallback_WhenMatchingEmailIsNotVerified_IsRejected()
    {
        using var services = await CreateServicesAsync();
        var userManager = services.GetRequiredService<UserManager<EventHub.Infrastructure.Data.Models.User>>();
        var service = services.GetRequiredService<IExternalAuthService>();
        await CreateUserAsync(userManager, "existing-user", "existing@example.com");
        var loginInfo = CreateLoginInfo("existing@example.com", "google-unverified", emailVerified: false);

        var result = await service.HandleExternalLoginCallbackAsync(loginInfo, null, "test-device");

        Assert.Equal(ExternalLoginProcessStatus.EmailNotVerified, result.Status);
    }

    [Fact]
    public async Task HandleExternalLoginCallback_WhenGoogleUserIsNew_RequiresConfirmation()
    {
        using var services = await CreateServicesAsync();
        var service = services.GetRequiredService<IExternalAuthService>();
        var loginInfo = CreateLoginInfo("new@example.com", "google-new");

        var result = await service.HandleExternalLoginCallbackAsync(loginInfo, null, "test-device");

        Assert.Equal(ExternalLoginProcessStatus.RequiresConfirmation, result.Status);
        Assert.Equal("new@example.com", result.Email);
    }

    [Fact]
    public async Task ConfirmExternalLogin_WhenNewUser_CreatesUserLoginAndClientRole()
    {
        using var services = await CreateServicesAsync();
        var userManager = services.GetRequiredService<UserManager<EventHub.Infrastructure.Data.Models.User>>();
        var service = services.GetRequiredService<IExternalAuthService>();
        var loginInfo = CreateLoginInfo("new@example.com", "google-new-confirmed");
        var model = new ExternalLoginConfirmationViewModel
        {
            UserName = "new-user",
            Email = "new@example.com",
            Provider = "Google"
        };

        var result = await service.ConfirmExternalLoginAsync(loginInfo, model, "127.0.0.1", "test-device");
        var created = await userManager.FindByNameAsync("new-user");
        var linked = await userManager.FindByLoginAsync("Google", "google-new-confirmed");
        IList<string> roles = created == null
            ? new List<string>()
            : await userManager.GetRolesAsync(created);

        Assert.True(result.Succeeded);
        Assert.NotNull(created);
        Assert.True(created!.EmailConfirmed);
        Assert.Equal(created.Id, linked?.Id);
        Assert.Contains("Client", roles);
    }

    [Fact]
    public async Task ConfirmExternalLogin_WhenUsernameAlreadyExists_ReturnsDuplicateUsername()
    {
        using var services = await CreateServicesAsync();
        var userManager = services.GetRequiredService<UserManager<EventHub.Infrastructure.Data.Models.User>>();
        var service = services.GetRequiredService<IExternalAuthService>();
        await CreateUserAsync(userManager, "taken-user", "taken@example.com");
        var loginInfo = CreateLoginInfo("new@example.com", "google-duplicate");
        var model = new ExternalLoginConfirmationViewModel
        {
            UserName = "taken-user",
            Email = "new@example.com",
            Provider = "Google"
        };

        var result = await service.ConfirmExternalLoginAsync(loginInfo, model, null, "test-device");

        Assert.Equal(ExternalLoginProcessStatus.DuplicateUserName, result.Status);
    }

    private static async Task<ServiceProvider> CreateServicesAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentity<EventHub.Infrastructure.Data.Models.User, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        services.AddScoped<IExternalAuthService, ExternalAuthService>();

        var provider = services.BuildServiceProvider();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        await roleManager.CreateAsync(new IdentityRole("Client"));

        return provider;
    }

    private static async Task<EventHub.Infrastructure.Data.Models.User> CreateUserAsync(
        UserManager<EventHub.Infrastructure.Data.Models.User> userManager,
        string userName,
        string email,
        bool isActive = true)
    {
        var user = new EventHub.Infrastructure.Data.Models.User
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            IsActive = isActive,
            IsDeleted = false
        };

        var result = await userManager.CreateAsync(user);
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));

        return user;
    }

    private static ExternalLoginInfo CreateLoginInfo(
        string email,
        string providerKey,
        bool emailVerified = true)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new("email_verified", emailVerified ? "true" : "false")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Google"));

        return new ExternalLoginInfo(principal, "Google", providerKey, "Google");
    }
}
