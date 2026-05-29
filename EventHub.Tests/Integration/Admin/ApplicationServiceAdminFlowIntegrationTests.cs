using EventHub.Core.Services;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventHub.Tests.Integration.Admin;

public class ApplicationServiceAdminFlowIntegrationTests
{
    [Fact]
    public async Task ApproveAsync_OrganizerApplication_PersistsOrganizerRole()
    {
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        services
            .AddIdentityCore<User>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Organizer"))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole("Organizer"));
            Assert.True(roleResult.Succeeded);
        }

        var user = CreateUser();
        var userResult = await userManager.CreateAsync(user);
        Assert.True(userResult.Succeeded);

        var application = CreateApplication(user, ApplicationType.Organizer, ApplicationStatus.Pending);
        db.ApplicationForms.Add(application);
        await db.SaveChangesAsync();

        var service = new ApplicationService(new Repository(db), userManager);
        var adminId = Guid.NewGuid().ToString();

        var result = await service.ApproveAsync(application.Id, adminId);

        var savedApplication = await db.ApplicationForms.SingleAsync(a => a.Id == application.Id);
        Assert.True(result);
        Assert.Equal(ApplicationStatus.Approved, savedApplication.Status);
        Assert.Equal(adminId, savedApplication.ReviewedById);
        Assert.NotNull(savedApplication.ReviewedAt);
        Assert.True(await userManager.IsInRoleAsync(user, "Organizer"));
    }

    [Fact]
    public async Task RejectAsync_PendingApplication_RejectsApplicationAndPersistsReviewData()
    {
        await using var db = CreateDbContext();
        var user = CreateUser();
        var application = CreateApplication(user, ApplicationType.Organizer, ApplicationStatus.Pending);
        db.Users.Add(user);
        db.ApplicationForms.Add(application);
        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();
        var service = CreateService(db, userManager);
        var adminId = Guid.NewGuid().ToString();
        const string comment = "Missing required business details.";

        var result = await service.RejectAsync(application.Id, adminId, comment);

        var savedApplication = await db.ApplicationForms.SingleAsync(a => a.Id == application.Id);
        Assert.True(result);
        Assert.Equal(ApplicationStatus.Rejected, savedApplication.Status);
        Assert.Equal(adminId, savedApplication.ReviewedById);
        Assert.Equal(comment, savedApplication.ReviewComment);
        Assert.NotNull(savedApplication.ReviewedAt);
        userManager.Verify(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RejectAsync_InvalidApplication_ReturnsFalseAndDoesNotChangeDatabase()
    {
        await using var db = CreateDbContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(db, userManager);

        var result = await service.RejectAsync(
            applicationId: 999,
            adminUserId: Guid.NewGuid().ToString(),
            comment: "No matching application.");

        Assert.False(result);
        Assert.Empty(await db.ApplicationForms.ToListAsync());
        userManager.Verify(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationService CreateService(
        ApplicationDbContext db,
        Mock<UserManager<User>> userManager)
        => new(new Repository(db), userManager.Object);

    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(
            store.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }

    private static User CreateUser()
        => new()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "applicant",
            Email = "applicant@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static ApplicationForm CreateApplication(
        User user,
        ApplicationType type,
        ApplicationStatus status)
        => new()
        {
            UserId = user.Id,
            User = user,
            Type = type,
            Status = status,
            OrganizationName = "Applicant Organization",
            PhoneNumber = "123456",
            Description = "Application description",
            CreatedAt = DateTime.UtcNow
        };
}
