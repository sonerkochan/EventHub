using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;

namespace EventHub.Tests.Unit.Admin;

[Trait("Category", "Unit")]
public class ApplicationServiceApprovalTests
{
    [Fact]
    public async Task ApproveAsync_OrganizerApplication_AssignsOrganizerRole()
    {
        const int applicationId = 1;
        const string adminId = "admin-user-id";
        var user = new User { Id = "user-id", UserName = "organizer-user" };
        var application = new ApplicationForm
        {
            Id = applicationId,
            UserId = user.Id,
            User = user,
            Type = ApplicationType.Organizer,
            Status = ApplicationStatus.Pending,
            OrganizationName = "Organizer Org",
            PhoneNumber = "123456"
        };
        var repository = CreateRepositoryMock([application]);
        var userManager = CreateUserManagerMock();
        userManager
            .Setup(u => u.IsInRoleAsync(user, "Organizer"))
            .ReturnsAsync(false);
        userManager
            .Setup(u => u.AddToRoleAsync(user, "Organizer"))
            .ReturnsAsync(IdentityResult.Success);
        var service = new ApplicationService(repository.Object, userManager.Object);

        var result = await service.ApproveAsync(applicationId, adminId);

        Assert.True(result);
        Assert.Equal(ApplicationStatus.Approved, application.Status);
        Assert.Equal(adminId, application.ReviewedById);
        Assert.NotNull(application.ReviewedAt);
        userManager.Verify(u => u.IsInRoleAsync(user, "Organizer"), Times.Once);
        userManager.Verify(u => u.AddToRoleAsync(user, "Organizer"), Times.Once);
        repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_SupplierApplication_AssignsSupplierRole()
    {
        const int applicationId = 2;
        const string adminId = "admin-user-id";
        var user = new User { Id = "user-id", UserName = "supplier-user" };
        var application = new ApplicationForm
        {
            Id = applicationId,
            UserId = user.Id,
            User = user,
            Type = ApplicationType.Supplier,
            Status = ApplicationStatus.Pending,
            OrganizationName = "Supplier Business",
            PhoneNumber = "123456"
        };
        var repository = CreateRepositoryMock([application]);
        var userManager = CreateUserManagerMock();
        userManager
            .Setup(u => u.IsInRoleAsync(user, "Supplier"))
            .ReturnsAsync(false);
        userManager
            .Setup(u => u.AddToRoleAsync(user, "Supplier"))
            .ReturnsAsync(IdentityResult.Success);
        var service = new ApplicationService(repository.Object, userManager.Object);

        var result = await service.ApproveAsync(applicationId, adminId);

        Assert.True(result);
        Assert.Equal(ApplicationStatus.Approved, application.Status);
        Assert.Equal(adminId, application.ReviewedById);
        Assert.NotNull(application.ReviewedAt);
        userManager.Verify(u => u.IsInRoleAsync(user, "Supplier"), Times.Once);
        userManager.Verify(u => u.AddToRoleAsync(user, "Supplier"), Times.Once);
        repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_AlreadyApproved_DoesNotAssignRoleTwice()
    {
        const int applicationId = 3;
        const string adminId = "admin-user-id";
        var user = new User { Id = "user-id", UserName = "organizer-user" };
        var application = new ApplicationForm
        {
            Id = applicationId,
            UserId = user.Id,
            User = user,
            Type = ApplicationType.Organizer,
            Status = ApplicationStatus.Approved,
            OrganizationName = "Organizer Org",
            PhoneNumber = "123456"
        };
        var repository = CreateRepositoryMock([application]);
        var userManager = CreateUserManagerMock();
        var service = new ApplicationService(repository.Object, userManager.Object);

        var result = await service.ApproveAsync(applicationId, adminId);

        Assert.False(result);
        userManager.Verify(u => u.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ApproveAsync_InvalidApplication_ReturnsFalse()
    {
        const int applicationId = 404;
        const string adminId = "admin-user-id";
        var repository = CreateRepositoryMock([]);
        var userManager = CreateUserManagerMock();
        var service = new ApplicationService(repository.Object, userManager.Object);

        var result = await service.ApproveAsync(applicationId, adminId);

        Assert.False(result);
        userManager.Verify(u => u.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    private static Mock<IRepository> CreateRepositoryMock(IEnumerable<ApplicationForm> applications)
    {
        var repository = new Mock<IRepository>();
        repository
            .Setup(r => r.All<ApplicationForm>())
            .Returns(applications.AsQueryable().BuildMock());
        repository
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        return repository;
    }

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
}
