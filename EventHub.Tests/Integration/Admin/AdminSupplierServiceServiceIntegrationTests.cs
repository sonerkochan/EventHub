using EventHub.Core.Contracts;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EventHub.Tests.Integration.Admin;

[Trait("Category", "Integration")]
public class AdminSupplierServiceServiceIntegrationTests
{
    [Fact]
    public async Task HideAsync_VisibleService_PersistsHiddenState()
    {
        await using var db = CreateDbContext();

        var supplierService = new SupplierService
        {
            Name = "Lighting package",
            Description = "Stage lighting",
            Price = 250m,
            SupplierId = Guid.NewGuid().ToString(),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        db.SupplierServices.Add(supplierService);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();

        var service = CreateService(db);

        var result = await service.HideAsync(supplierService.Id);

        db.ChangeTracker.Clear();

        var savedService = await db.SupplierServices
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleAsync(s => s.Id == supplierService.Id);

        Assert.True(result);
        Assert.True(savedService.IsDeleted);
        Assert.NotNull(savedService.DeletedAt);
        Assert.NotEqual(default, savedService.UpdatedAt);
    }

    [Fact]
    public async Task UnhideAsync_HiddenService_PersistsVisibleState()
    {
        await using var db = CreateDbContext();

        var supplierService = new SupplierService
        {
            Name = "Hidden audio package",
            Description = "Audio equipment",
            Price = 400m,
            SupplierId = Guid.NewGuid().ToString(),
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow
        };

        db.SupplierServices.Add(supplierService);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();

        var service = CreateService(db);

        var result = await service.UnhideAsync(supplierService.Id);

        db.ChangeTracker.Clear();

        var savedService = await db.SupplierServices
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleAsync(s => s.Id == supplierService.Id);

        Assert.True(result);
        Assert.False(savedService.IsDeleted);
        Assert.Null(savedService.DeletedAt);
        Assert.NotEqual(default, savedService.UpdatedAt);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static AdminSupplierServiceService CreateService(ApplicationDbContext db)
        => new(
            new Repository(db),
            new Mock<ICurrencyDisplayService>().Object);
}