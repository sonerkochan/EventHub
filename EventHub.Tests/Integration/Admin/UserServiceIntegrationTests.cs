using EventHub.Core.Models.User;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventHub.Tests.Integration.Admin;

[Trait("Category", "Integration")]
public class UserServiceIntegrationTests
{
    [Fact]
    public async Task CreateUserAsync_ValidModel_PersistsUserAndAssignedRole()
    {
        await using var provider = CreateServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedRoleAsync(roleManager, "Client");
        var service = CreateService(db, userManager, roleManager);
        var model = CreateUserModel(role: "Client");

        var result = await service.CreateUserAsync(model);

        var savedUser = await db.Users.SingleAsync(u => u.UserName == model.UserName);
        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.Equal(model.Email, savedUser.Email);
        Assert.Equal(model.FirstName, savedUser.FirstName);
        Assert.Equal(model.LastName, savedUser.LastName);
        Assert.Equal(model.PhoneNumber, savedUser.PhoneNumber);
        Assert.True(savedUser.IsActive);
        Assert.False(savedUser.IsDeleted);
        Assert.True(await userManager.IsInRoleAsync(savedUser, "Client"));
    }

    [Fact]
    public async Task UpdateUserAsync_ExistingUser_PersistsUpdatedFields()
    {
        await using var provider = CreateServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var user = await CreateUserAsync(userManager);
        var service = CreateService(db, userManager, roleManager);
        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = "updated@example.com",
            FirstName = "Updated",
            LastName = "User",
            PhoneNumber = "999999",
            IsActive = false
        };

        var result = await service.UpdateUserAsync(model);

        var savedUser = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.Equal(model.Email, savedUser.Email);
        Assert.Equal(model.FirstName, savedUser.FirstName);
        Assert.Equal(model.LastName, savedUser.LastName);
        Assert.Equal(model.PhoneNumber, savedUser.PhoneNumber);
        Assert.False(savedUser.IsActive);
        Assert.False(savedUser.EmailConfirmed);
    }

    [Fact]
    public async Task DeactivateUserAsync_ExistingUser_PersistsInactiveStatus()
    {
        await using var provider = CreateServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var user = await CreateUserAsync(userManager, isActive: true);
        var service = CreateService(db, userManager, roleManager);

        var result = await service.DeactivateUserAsync(user.Id);

        var savedUser = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.True(result);
        Assert.False(savedUser.IsActive);
        Assert.NotEqual(default, savedUser.UpdatedAt);
    }

    [Fact]
    public async Task ReactivateUserAsync_ExistingUser_PersistsActiveStatus()
    {
        await using var provider = CreateServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var user = await CreateUserAsync(userManager, isActive: false);
        var service = CreateService(db, userManager, roleManager);

        var result = await service.ReactivateUserAsync(user.Id);

        var savedUser = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.True(result);
        Assert.True(savedUser.IsActive);
        Assert.NotEqual(default, savedUser.UpdatedAt);
    }

    [Fact]
    public async Task DeleteUserAsync_ExistingUser_PersistsSoftDeleteState()
    {
        await using var provider = CreateServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var user = await CreateUserAsync(userManager);
        var service = CreateService(db, userManager, roleManager);

        var result = await service.DeleteUserAsync(user.Id);

        var savedUser = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.True(result);
        Assert.True(savedUser.IsDeleted);
        Assert.NotNull(savedUser.DeletedAt);
        Assert.NotEqual(default, savedUser.UpdatedAt);
    }

    [Fact]
    public async Task AddRoleToUserAsync_ExistingUserAndRole_PersistsRoleAssignment()
    {
        await using var provider = CreateServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedRoleAsync(roleManager, "Organizer");
        var user = await CreateUserAsync(userManager);
        var service = CreateService(db, userManager, roleManager);

        var result = await service.AddRoleToUserAsync(user.Id, "Organizer");

        var savedUser = await userManager.FindByIdAsync(user.Id);
        Assert.True(result);
        Assert.NotNull(savedUser);
        Assert.True(await userManager.IsInRoleAsync(savedUser, "Organizer"));
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_ExistingUserWithRole_RemovesRoleAssignment()
    {
        await using var provider = CreateServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedRoleAsync(roleManager, "Organizer");
        var user = await CreateUserAsync(userManager);
        var addRoleResult = await userManager.AddToRoleAsync(user, "Organizer");
        Assert.True(addRoleResult.Succeeded);
        var service = CreateService(db, userManager, roleManager);

        var result = await service.RemoveRoleFromUserAsync(user.Id, "Organizer");

        var savedUser = await userManager.FindByIdAsync(user.Id);
        Assert.True(result);
        Assert.NotNull(savedUser);
        Assert.False(await userManager.IsInRoleAsync(savedUser, "Organizer"));
    }

    [Fact]
    public async Task DeactivateUserAsync_MissingUser_ReturnsFalseAndDoesNotCreateRows()
    {
        await using var provider = CreateServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var service = CreateService(db, userManager, roleManager);

        var result = await service.DeactivateUserAsync("missing-user-id");

        Assert.False(result);
        Assert.Empty(await db.Users.ToListAsync());
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

    private static UserService CreateService(
        ApplicationDbContext db,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
        => new(userManager, roleManager, new Repository(db));

    private static async Task SeedRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        Assert.True(result.Succeeded);
    }

    private static async Task<User> CreateUserAsync(
        UserManager<User> userManager,
        bool isActive = true)
    {
        var user = new User
        {
            UserName = $"user-{Guid.NewGuid():N}",
            Email = $"user-{Guid.NewGuid():N}@example.com",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890",
            IsActive = isActive,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, "Password123");
        Assert.True(result.Succeeded);
        return user;
    }

    private static CreateUserViewModel CreateUserModel(string? role = null)
    {
        var unique = Guid.NewGuid().ToString("N");
        return new CreateUserViewModel
        {
            UserName = $"user-{unique}",
            Email = $"user-{unique}@example.com",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Role = role
        };
    }
}
