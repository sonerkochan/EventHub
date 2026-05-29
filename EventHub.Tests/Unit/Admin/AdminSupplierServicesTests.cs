using EventHub.Core.Contracts;
using EventHub.Core.Models.Currency;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using MockQueryable.Moq;
using Moq;

namespace EventHub.Tests.Unit.Admin;

public class AdminSupplierServicesTests
{
    [Fact]
    public async Task GetAllAsync_WithVisibleFilter_ReturnsOnlyVisibleServicesWithFormattedPrices()
    {
        var supplier = new User
        {
            Id = "supp1",
            FirstName = "First_name",
            LastName = "Last_name",
            Email = "sup1@supplier.com"
        };
        var visibleService = new SupplierService
        {
            Id = 1,
            Name = "Lighting",
            Description = "Stage lighting",
            Price = 120m,
            SupplierId = supplier.Id,
            CreatedAt = new DateTime(2026, 5, 1),
            IsDeleted = false
        };
        var hiddenService = new SupplierService
        {
            Id = 2,
            Name = "Audio",
            Price = 80m,
            SupplierId = supplier.Id,
            CreatedAt = new DateTime(2026, 5, 2),
            IsDeleted = true
        };

        var currency = CreateCurrencyDisplayMock();
        var service = CreateService(
            [visibleService, hiddenService],
            [supplier],
            [],
            currency.Object);

        var result = (await service.GetAllAsync(statusFilter: "visible")).ToList();

        Assert.Single(result);
        Assert.Equal(visibleService.Id, result[0].Id);
        Assert.Equal("First_name Last_name", result[0].SupplierName);
        
        Assert.True(
            result[0].PriceText == "120,00 EUR" ||
            result[0].PriceText == "120.00 EUR",
            $"Expected price text to be either '120,00 EUR' or '120.00 EUR', but was '{result[0].PriceText}'.");

        currency.Verify(c => c.FormatAsync(120m, null), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithHiddenFilter_ReturnsOnlyHiddenServices()
    {
        var visibleService = new SupplierService
        {
            Id = 1,
            Name = "Lighting",
            Price = 120m,
            CreatedAt = new DateTime(2026, 5, 1),
            IsDeleted = false
        };
        var hiddenService = new SupplierService
        {
            Id = 2,
            Name = "Hidden audio",
            Price = 80m,
            CreatedAt = new DateTime(2026, 5, 2),
            IsDeleted = true,
            DeletedAt = new DateTime(2026, 5, 3)
        };

        var service = CreateService([visibleService, hiddenService], [], []);

        var result = (await service.GetAllAsync(statusFilter: "hidden")).ToList();

        Assert.Single(result);
        Assert.Equal(hiddenService.Id, result[0].Id);
        Assert.True(result[0].IsHidden);
        Assert.Equal(hiddenService.DeletedAt, result[0].HiddenAt);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchTerm_FiltersByNameOrDescriptionAndAddsRequestStats()
    {
        var serviceOne = new SupplierService
        {
            Id = 1,
            Name = "Premium Lighting",
            Description = "Large event setup",
            Price = 100m,
            CreatedAt = new DateTime(2026, 5, 1)
        };
        var serviceTwo = new SupplierService
        {
            Id = 2,
            Name = "Audio Desk",
            Description = "Lighting compatible console",
            Price = 200m,
            CreatedAt = new DateTime(2026, 5, 2)
        };
        var nonMatch = new SupplierService
        {
            Id = 3,
            Name = "Catering",
            Description = "Food",
            Price = 300m,
            CreatedAt = new DateTime(2026, 5, 3)
        };
        var requests = new[]
        {
            new ServiceRentalRequest { Id = 1, SupplierServiceId = 1, Status = ServiceRentalRequestStatus.Pending },
            new ServiceRentalRequest { Id = 2, SupplierServiceId = 1, Status = ServiceRentalRequestStatus.Accepted },
            new ServiceRentalRequest { Id = 3, SupplierServiceId = 2, Status = ServiceRentalRequestStatus.Pending },
            new ServiceRentalRequest { Id = 4, SupplierServiceId = 3, Status = ServiceRentalRequestStatus.Pending }
        };

        var service = CreateService([serviceOne, serviceTwo, nonMatch], [], requests);

        var result = (await service.GetAllAsync(searchTerm: " Lighting ")).ToList();

        Assert.Equal([2, 1], result.Select(r => r.Id));
        Assert.Equal(1, result.Single(r => r.Id == 1).PendingRequestCount);
        Assert.Equal(2, result.Single(r => r.Id == 1).TotalRequestCount);
        Assert.Equal(1, result.Single(r => r.Id == 2).PendingRequestCount);
        Assert.Equal(1, result.Single(r => r.Id == 2).TotalRequestCount);
    }

    [Fact]
    public async Task HideAsync_WhenServiceIsVisible_MarksServiceAsHiddenAndSavesChanges()
    {
        var supplierService = new SupplierService
        {
            Id = 7,
            Name = "Lighting",
            IsDeleted = false
        };
        var repo = CreateRepositoryMock([supplierService], [], []);
        var service = new AdminSupplierServiceService(repo.Object, CreateCurrencyDisplayMock().Object);

        var result = await service.HideAsync(supplierService.Id);

        Assert.True(result);
        Assert.True(supplierService.IsDeleted);
        Assert.NotNull(supplierService.DeletedAt);
        repo.Verify(r => r.Update(supplierService), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task HideAsync_WhenServiceIsAlreadyHidden_ReturnsFalseWithoutSaving()
    {
        var supplierService = new SupplierService
        {
            Id = 7,
            Name = "Lighting",
            IsDeleted = true
        };
        var repo = CreateRepositoryMock([supplierService], [], []);
        var service = new AdminSupplierServiceService(repo.Object, CreateCurrencyDisplayMock().Object);

        var result = await service.HideAsync(supplierService.Id);

        Assert.False(result);
        repo.Verify(r => r.Update(It.IsAny<SupplierService>()), Times.Never);
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UnhideAsync_WhenServiceIsHidden_ClearsHiddenStateAndSavesChanges()
    {
        var supplierService = new SupplierService
        {
            Id = 7,
            Name = "Lighting",
            IsDeleted = true,
            DeletedAt = new DateTime(2026, 5, 3)
        };
        var repo = CreateRepositoryMock([supplierService], [], []);
        var service = new AdminSupplierServiceService(repo.Object, CreateCurrencyDisplayMock().Object);

        var result = await service.UnhideAsync(supplierService.Id);

        Assert.True(result);
        Assert.False(supplierService.IsDeleted);
        Assert.Null(supplierService.DeletedAt);
        repo.Verify(r => r.Update(supplierService), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UnhideAsync_WhenServiceIsAlreadyVisible_ReturnsFalseWithoutSaving()
    {
        var supplierService = new SupplierService
        {
            Id = 7,
            Name = "Lighting",
            IsDeleted = false
        };
        var repo = CreateRepositoryMock([supplierService], [], []);
        var service = new AdminSupplierServiceService(repo.Object, CreateCurrencyDisplayMock().Object);

        var result = await service.UnhideAsync(supplierService.Id);

        Assert.False(result);
        repo.Verify(r => r.Update(It.IsAny<SupplierService>()), Times.Never);
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    private static AdminSupplierServiceService CreateService(
        IEnumerable<SupplierService> services,
        IEnumerable<User> users,
        IEnumerable<ServiceRentalRequest> requests,
        ICurrencyDisplayService? currencyDisplayService = null)
    {
        var repo = CreateRepositoryMock(services, users, requests);
        return new AdminSupplierServiceService(
            repo.Object,
            currencyDisplayService ?? CreateCurrencyDisplayMock().Object);
    }

    private static Mock<IRepository> CreateRepositoryMock(
        IEnumerable<SupplierService> services,
        IEnumerable<User> users,
        IEnumerable<ServiceRentalRequest> requests)
    {
        var repo = new Mock<IRepository>();

        repo.Setup(r => r.AllReadonly<SupplierService>())
            .Returns(services.AsQueryable().BuildMock());
        repo.Setup(r => r.All<SupplierService>())
            .Returns(services.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<User>())
            .Returns(users.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<ServiceRentalRequest>())
            .Returns(requests.AsQueryable().BuildMock());
        repo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        return repo;
    }

    private static Mock<ICurrencyDisplayService> CreateCurrencyDisplayMock()
    {
        var currency = new Mock<ICurrencyDisplayService>();

        currency
            .Setup(c => c.FormatAsync(It.IsAny<decimal>(), It.IsAny<string?>()))
            .ReturnsAsync((decimal amount, string? _) => new CurrencyDisplayValue
            {
                Amount = amount,
                Currency = "EUR",
                Text = $"{amount:0.00} EUR"
            });

        return currency;
    }
}
