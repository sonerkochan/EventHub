using EventHub.Core.Contracts;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EventHub.Tests.Integration.Supplier;

public class SupplierServiceCatalogServiceIntegrationTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task RequestServiceAsync_WithValidData_ShouldSaveRequestInDatabase()
    {
        using var dbContext = CreateDbContext();

        dbContext.SupplierServices.Add(new SupplierService
        {
            Id = 1,
            Name = "Sound System",
            SupplierId = "supplier-1"
        });

        await dbContext.SaveChangesAsync();

        var repo = new Repository(dbContext);
        var currencyMock = new Mock<ICurrencyDisplayService>();

        var service = new SupplierServiceCatalogService(
            repo,
            currencyMock.Object);

        var result = await service.RequestServiceAsync(
            1,
            "user-1",
            "  Need this for event  ");

        var request = await dbContext.ServiceRentalRequests.FirstOrDefaultAsync();

        Assert.True(result);
        Assert.NotNull(request);
        Assert.Equal(1, request!.SupplierServiceId);
        Assert.Equal("user-1", request.RequesterId);
        Assert.Equal("Need this for event", request.Message);
        Assert.Equal(ServiceRentalRequestStatus.Pending, request.Status);
    }

    [Fact]
    public async Task AcceptRequestAsync_WithValidPendingRequest_ShouldUpdateRequestInDatabase()
    {
        using var dbContext = CreateDbContext();

        var supplierService = new SupplierService
        {
            Id = 2,
            Name = "Lighting",
            SupplierId = "supplier-1"
        };

        dbContext.SupplierServices.Add(supplierService);

        dbContext.ServiceRentalRequests.Add(new ServiceRentalRequest
        {
            Id = 10,
            SupplierServiceId = 2,
            RequesterId = "user-1",
            Status = ServiceRentalRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var repo = new Repository(dbContext);
        var currencyMock = new Mock<ICurrencyDisplayService>();

        var service = new SupplierServiceCatalogService(
            repo,
            currencyMock.Object);

        var result = await service.AcceptRequestAsync(
            10,
            "supplier-1",
            "reviewer-1",
            " Accepted ");

        var request = await dbContext.ServiceRentalRequests.FirstAsync(r => r.Id == 10);

        Assert.True(result);
        Assert.Equal(ServiceRentalRequestStatus.Accepted, request.Status);
        Assert.Equal("reviewer-1", request.ReviewedById);
        Assert.Equal("Accepted", request.ResponseComment);
        Assert.NotNull(request.ReviewedAt);
    }

    [Fact]
    public async Task SearchServicesAsync_WhenSearchTermMatchesName_ShouldReturnMatchingServices()
    {
        using var dbContext = CreateDbContext();

        dbContext.SupplierServices.AddRange(
            new SupplierService
            {
                Id = 1,
                Name = "Sound System",
                Description = "Professional audio equipment",
                Price = 300m,
                SupplierId = "supplier-1"
            },
            new SupplierService
            {
                Id = 2,
                Name = "Catering",
                Description = "Food and drinks",
                Price = 500m,
                SupplierId = "supplier-2"
            }
        );

        await dbContext.SaveChangesAsync();

        var repo = new Repository(dbContext);
        var currencyMock = new Mock<ICurrencyDisplayService>();

        currencyMock
            .Setup(c => c.FormatAsync(300m, null))
            .ReturnsAsync(new EventHub.Core.Models.Currency.CurrencyDisplayValue
            {
                Text = "300.00 лв."
            });

        var service = new SupplierServiceCatalogService(
            repo,
            currencyMock.Object);

        var result = await service.SearchServicesAsync("  Sound  ", "user-1");

        Assert.Equal("Sound", result.SearchTerm);
        Assert.Single(result.Services);

        var foundService = result.Services.First();

        Assert.Equal("Sound System", foundService.Name);
        Assert.Equal("Professional audio equipment", foundService.Description);
        Assert.Equal(300m, foundService.Price);
        Assert.Equal("300.00 лв.", foundService.PriceText);
    }

    [Fact]
    public async Task AcceptRequestAsync_WhenRequestBelongsToAnotherSupplier_ShouldReturnFalse()
    {
        using var dbContext = CreateDbContext();

        dbContext.SupplierServices.Add(new SupplierService
        {
            Id = 1,
            Name = "Lighting",
            SupplierId = "real-supplier"
        });

        dbContext.ServiceRentalRequests.Add(new ServiceRentalRequest
        {
            Id = 1,
            SupplierServiceId = 1,
            RequesterId = "user-1",
            Status = ServiceRentalRequestStatus.Pending
        });

        await dbContext.SaveChangesAsync();

        var repo = new Repository(dbContext);
        var currencyMock = new Mock<ICurrencyDisplayService>();

        var service = new SupplierServiceCatalogService(
            repo,
            currencyMock.Object);

        var result = await service.AcceptRequestAsync(
            1,
            "fake-supplier",
            "reviewer-1",
            "Accepted");

        var request = await dbContext.ServiceRentalRequests.FirstAsync();

        Assert.False(result);
        Assert.Equal(ServiceRentalRequestStatus.Pending, request.Status);
        Assert.Null(request.ReviewedAt);
    }

    [Fact]
    public async Task AcceptRequestAsync_WhenRequestAlreadyAccepted_ShouldReturnFalse()
    {
        using var dbContext = CreateDbContext();

        dbContext.SupplierServices.Add(new SupplierService
        {
            Id = 2,
            Name = "Catering",
            SupplierId = "supplier-1"
        });

        dbContext.ServiceRentalRequests.Add(new ServiceRentalRequest
        {
            Id = 2,
            SupplierServiceId = 2,
            RequesterId = "user-1",
            Status = ServiceRentalRequestStatus.Accepted
        });

        await dbContext.SaveChangesAsync();

        var repo = new Repository(dbContext);
        var currencyMock = new Mock<ICurrencyDisplayService>();

        var service = new SupplierServiceCatalogService(
            repo,
            currencyMock.Object);

        var result = await service.AcceptRequestAsync(
            2,
            "supplier-1",
            "reviewer-1",
            "Accepted again");

        Assert.False(result);
    }

    [Fact]
    public async Task RequestServiceAsync_WhenRequestingOwnService_ShouldReturnFalse()
    {
        using var dbContext = CreateDbContext();

        dbContext.SupplierServices.Add(new SupplierService
        {
            Id = 3,
            Name = "DJ Setup",
            SupplierId = "supplier-1"
        });

        await dbContext.SaveChangesAsync();

        var repo = new Repository(dbContext);
        var currencyMock = new Mock<ICurrencyDisplayService>();

        var service = new SupplierServiceCatalogService(
            repo,
            currencyMock.Object);

        var result = await service.RequestServiceAsync(
            3,
            "supplier-1",
            "Need my own service");

        Assert.False(result);

        var requestsCount = await dbContext.ServiceRentalRequests.CountAsync();

        Assert.Equal(0, requestsCount);
    }

    [Fact]
    public async Task GetRequestsForSupplierAsync_ShouldReturnOnlyRequestsForGivenSupplier()
    {
        using var dbContext = CreateDbContext();

        dbContext.Users.AddRange(
            new User
            {
                Id = "user-1",
                UserName = "user1@test.com",
                Email = "user1@test.com",
                FirstName = "Ivan",
                LastName = "Ivanov"
            },
            new User
            {
                Id = "user-2",
                UserName = "user2@test.com",
                Email = "user2@test.com",
                FirstName = "Petar",
                LastName = "Petrov"
            }
        );

        dbContext.SupplierServices.AddRange(
            new SupplierService
            {
                Id = 1,
                Name = "Sound System",
                Price = 300m,
                SupplierId = "supplier-1"
            },
            new SupplierService
            {
                Id = 2,
                Name = "Catering",
                Price = 500m,
                SupplierId = "supplier-2"
            }
        );

        dbContext.ServiceRentalRequests.AddRange(
            new ServiceRentalRequest
            {
                Id = 1,
                SupplierServiceId = 1,
                RequesterId = "user-1",
                Status = ServiceRentalRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            },
            new ServiceRentalRequest
            {
                Id = 2,
                SupplierServiceId = 2,
                RequesterId = "user-2",
                Status = ServiceRentalRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            }
        );

        await dbContext.SaveChangesAsync();

        var repo = new Repository(dbContext);
        var currencyMock = new Mock<ICurrencyDisplayService>();

        currencyMock
            .Setup(c => c.FormatAsync(300m, null))
            .ReturnsAsync(new EventHub.Core.Models.Currency.CurrencyDisplayValue
            {
                Text = "300.00 лв."
            });

        var service = new SupplierServiceCatalogService(
            repo,
            currencyMock.Object);

        var result = await service.GetRequestsForSupplierAsync("supplier-1");

        Assert.Single(result);

        var request = result.First();

        Assert.Equal(1, request.Id);
        Assert.Equal("Sound System", request.ServiceName);
        Assert.Equal("Ivan Ivanov", request.RequesterName);
        Assert.Equal("user1@test.com", request.RequesterEmail);
        Assert.Equal(300m, request.Price);
        Assert.Equal("300.00 лв.", request.PriceText);
    }

    [Fact]
    public async Task GetRequestsForSupplierAsync_WhenRequesterNameIsMissing_ShouldUseEmailAsRequesterName()
    {
        using var dbContext = CreateDbContext();

        dbContext.Users.Add(new User
        {
            Id = "user-1",
            UserName = "user1@test.com",
            Email = "user1@test.com",
            FirstName = null,
            LastName = null
        });

        dbContext.SupplierServices.Add(new SupplierService
        {
            Id = 1,
            Name = "Lighting",
            Price = null,
            SupplierId = "supplier-1"
        });

        dbContext.ServiceRentalRequests.Add(new ServiceRentalRequest
        {
            Id = 1,
            SupplierServiceId = 1,
            RequesterId = "user-1",
            Status = ServiceRentalRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var repo = new Repository(dbContext);
        var currencyMock = new Mock<ICurrencyDisplayService>();

        var service = new SupplierServiceCatalogService(
            repo,
            currencyMock.Object);

        var result = await service.GetRequestsForSupplierAsync("supplier-1");

        Assert.Single(result);

        var request = result.First();

        Assert.Equal("Lighting", request.ServiceName);
        Assert.Equal("user1@test.com", request.RequesterName);
        Assert.Equal("user1@test.com", request.RequesterEmail);
        Assert.Null(request.Price);
        Assert.Equal(string.Empty, request.PriceText);
    }

    [Fact]
    public async Task SearchServicesAsync_WhenSearchTermMatchesDescription_ShouldReturnMatchingServices()
    {
        using var dbContext = CreateDbContext();

        dbContext.SupplierServices.AddRange(
            new SupplierService
            {
                Id = 1,
                Name = "Lighting",
                Description = "Professional audio setup",
                SupplierId = "supplier-1"
            },
            new SupplierService
            {
                Id = 2,
                Name = "Catering",
                Description = "Food services",
                SupplierId = "supplier-2"
            }
        );

        await dbContext.SaveChangesAsync();

        var repo = new Repository(dbContext);
        var currencyMock = new Mock<ICurrencyDisplayService>();

        var service = new SupplierServiceCatalogService(
            repo,
            currencyMock.Object);

        var result = await service.SearchServicesAsync("audio", "user-1");

        Assert.Single(result.Services);

        var foundService = result.Services.First();

        Assert.Equal("Lighting", foundService.Name);
        Assert.Equal("Professional audio setup", foundService.Description);
    }

    [Fact]
    public async Task SearchServicesAsync_ShouldIncludeLatestUserRequestStatus()
    {
        using var dbContext = CreateDbContext();

        dbContext.SupplierServices.Add(new SupplierService
        {
            Id = 1,
            Name = "Sound System",
            SupplierId = "supplier-1"
        });

        dbContext.ServiceRentalRequests.AddRange(
            new ServiceRentalRequest
            {
                Id = 1,
                SupplierServiceId = 1,
                RequesterId = "user-1",
                Status = ServiceRentalRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow.AddHours(-2)
            },
            new ServiceRentalRequest
            {
                Id = 2,
                SupplierServiceId = 1,
                RequesterId = "user-1",
                Status = ServiceRentalRequestStatus.Accepted,
                RequestedAt = DateTime.UtcNow
            }
        );

        await dbContext.SaveChangesAsync();

        var repo = new Repository(dbContext);
        var currencyMock = new Mock<ICurrencyDisplayService>();

        var service = new SupplierServiceCatalogService(
            repo,
            currencyMock.Object);

        var result = await service.SearchServicesAsync(null, "user-1");

        Assert.Single(result.Services);

        var serviceItem = result.Services.First();

        Assert.Equal(2, serviceItem.CurrentUserRequestId);
        Assert.Equal(
            ServiceRentalRequestStatus.Accepted,
            serviceItem.CurrentUserRequestStatus);
    }

    [Fact]
    public async Task DeclineRequestAsync_WithValidPendingRequest_ShouldUpdateRequestInDatabase()
    {
        using var dbContext = CreateDbContext();

        dbContext.SupplierServices.Add(new SupplierService
        {
            Id = 1,
            Name = "DJ Setup",
            SupplierId = "supplier-1"
        });

        dbContext.ServiceRentalRequests.Add(new ServiceRentalRequest
        {
            Id = 1,
            SupplierServiceId = 1,
            RequesterId = "user-1",
            Status = ServiceRentalRequestStatus.Pending
        });

        await dbContext.SaveChangesAsync();

        var repo = new Repository(dbContext);
        var currencyMock = new Mock<ICurrencyDisplayService>();

        var service = new SupplierServiceCatalogService(
            repo,
            currencyMock.Object);

        var result = await service.DeclineRequestAsync(
            1,
            "supplier-1",
            "reviewer-1",
            "Declined");

        var request = await dbContext.ServiceRentalRequests.FirstAsync();

        Assert.True(result);
        Assert.Equal(ServiceRentalRequestStatus.Declined, request.Status);
        Assert.Equal("reviewer-1", request.ReviewedById);
        Assert.Equal("Declined", request.ResponseComment);
        Assert.NotNull(request.ReviewedAt);
    }

    [Fact]
    public async Task DeclineRequestAsync_WhenRequestAlreadyDeclined_ShouldReturnFalse()
    {
        using var dbContext = CreateDbContext();

        dbContext.SupplierServices.Add(new SupplierService
        {
            Id = 1,
            Name = "Lighting",
            SupplierId = "supplier-1"
        });

        dbContext.ServiceRentalRequests.Add(new ServiceRentalRequest
        {
            Id = 1,
            SupplierServiceId = 1,
            RequesterId = "user-1",
            Status = ServiceRentalRequestStatus.Declined
        });

        await dbContext.SaveChangesAsync();

        var repo = new Repository(dbContext);
        var currencyMock = new Mock<ICurrencyDisplayService>();

        var service = new SupplierServiceCatalogService(
            repo,
            currencyMock.Object);

        var result = await service.DeclineRequestAsync(
            1,
            "supplier-1",
            "reviewer-1",
            "Declined again");

        Assert.False(result);
    }
}