using EventHub.Core.Models.Moderator;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventHub.Tests.Integration.Admin;

[Trait("Category", "Integration")]
public class ModeratorServiceIntegrationTests
{
    [Fact]
    public async Task CreateModeratorAsync_ValidModel_PersistsUserAndModeratorRole()
    {
        await using var provider = CreateServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedRoleAsync(roleManager, "Moderator");
        var service = new ModeratorService(userManager);
        var model = CreateAddModeratorViewModel();

        var result = await service.CreateModeratorAsync(model);

        var savedUser = await db.Users.SingleAsync(u => u.UserName == model.Username);
        Assert.True(result);
        Assert.Equal(model.Email, savedUser.Email);
        Assert.Equal(model.FirstName, savedUser.FirstName);
        Assert.Equal(model.LastName, savedUser.LastName);
        Assert.True(savedUser.IsActive);
        Assert.NotEqual(default, savedUser.CreatedAt);
        Assert.NotEqual(default, savedUser.UpdatedAt);
        Assert.True(await userManager.IsInRoleAsync(savedUser, "Moderator"));
    }

    [Fact]
    public async Task SetActiveStatusAsync_WhenModeratorExists_DeactivatesUserAndPersistsChange()
    {
        await using var provider = CreateServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await CreateModeratorUserAsync(userManager, isActive: true);
        var service = new ModeratorService(userManager);

        var result = await service.SetActiveStatusAsync(user.Id, false);

        var savedUser = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.True(result);
        Assert.False(savedUser.IsActive);
        Assert.NotEqual(default, savedUser.UpdatedAt);
    }

    [Fact]
    public async Task SetActiveStatusAsync_WhenModeratorExists_ActivatesUserAndPersistsChange()
    {
        await using var provider = CreateServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await CreateModeratorUserAsync(userManager, isActive: false);
        var service = new ModeratorService(userManager);

        var result = await service.SetActiveStatusAsync(user.Id, true);

        var savedUser = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.True(result);
        Assert.True(savedUser.IsActive);
        Assert.NotEqual(default, savedUser.UpdatedAt);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services
            .AddIdentityCore<User>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        return services.BuildServiceProvider();
    }

    private static async Task SeedRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        Assert.True(result.Succeeded);
    }

    private static async Task<User> CreateModeratorUserAsync(UserManager<User> userManager, bool isActive)
    {
        var user = new User
        {
            UserName = $"moderator-{Guid.NewGuid():N}",
            Email = $"moderator-{Guid.NewGuid():N}@example.com",
            FirstName = "Test",
            LastName = "Moderator",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, "Password123");
        Assert.True(result.Succeeded);
        return user;
    }

    private static AddModeratorViewModel CreateAddModeratorViewModel()
        => new()
        {
            Username = $"moderator-{Guid.NewGuid():N}",
            Email = $"moderator-{Guid.NewGuid():N}@example.com",
            FirstName = "Test",
            LastName = "Moderator",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };
}
