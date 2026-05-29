using EventHub.Core.Models.Venue;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Tests.Integration.Admin;

public class VenueServiceAdminFlowIntegrationTests
{
    [Fact]
    public async Task AddVenueAsync_ValidVenue_PersistsVenue()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var userId = Guid.NewGuid();
        var model = CreateAddVenueViewModel();

        await service.AddVenueAsync(model, userId);

        var savedVenue = await db.Venues.SingleAsync();
        Assert.Equal(userId, savedVenue.CreatedBy);
        Assert.Equal(model.Name, savedVenue.Name);
        Assert.Equal(model.Description, savedVenue.Description);
        Assert.Equal(model.Address, savedVenue.Address);
        Assert.Equal(model.City, savedVenue.City);
        Assert.Equal(model.Country, savedVenue.Country);
        Assert.Equal(model.PostalCode, savedVenue.PostalCode);
        Assert.Equal(model.Latitude, savedVenue.Latitude);
        Assert.Equal(model.Longitude, savedVenue.Longitude);
        Assert.Equal(model.ContactEmail, savedVenue.ContactEmail);
        Assert.Equal(model.ContactPhone, savedVenue.ContactPhone);
        Assert.True(savedVenue.IsActive);
        Assert.NotEqual(default, savedVenue.CreatedAt);
        Assert.NotEqual(default, savedVenue.UpdatedAt);
    }

    [Fact]
    public async Task GetAllVenuesAsync_ReturnsOnlyActiveVenuesFromDatabase()
    {
        await using var db = CreateDbContext();
        var activeVenue = SeedVenue(db, name: "Active Venue", isActive: true);
        SeedVenue(db, name: "Inactive Venue", isActive: false);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = (await service.GetAllVenuesAsync()).ToList();

        Assert.Single(result);
        Assert.Equal(activeVenue.Id, result[0].Id);
        Assert.Equal(activeVenue.Name, result[0].Name);
        Assert.Equal(activeVenue.City, result[0].City);
        Assert.Equal(activeVenue.Country, result[0].Country);
        Assert.Equal(activeVenue.Address, result[0].Address);
    }

    [Fact]
    public async Task GetByIdAsync_ActiveVenue_ReturnsVenueDetail()
    {
        await using var db = CreateDbContext();
        var venue = SeedVenue(db);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.GetByIdAsync(venue.Id);

        Assert.NotNull(result);
        Assert.Equal(venue.Id, result.Id);
        Assert.Equal(venue.Name, result.Name);
        Assert.Equal(venue.Description, result.Description);
        Assert.Equal(venue.Address, result.Address);
        Assert.Equal(venue.City, result.City);
        Assert.Equal(venue.Country, result.Country);
        Assert.Equal(venue.PostalCode, result.PostalCode);
        Assert.Equal(venue.ContactEmail, result.ContactEmail);
        Assert.Equal(venue.ContactPhone, result.ContactPhone);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetForEditAsync_ExistingVenue_ReturnsEditModelFromDatabase()
    {
        await using var db = CreateDbContext();
        var venue = SeedVenue(db);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.GetForEditAsync(venue.Id);

        Assert.NotNull(result);
        Assert.Equal(venue.Id, result.Id);
        Assert.Equal(venue.Name, result.Name);
        Assert.Equal(venue.Description, result.Description);
        Assert.Equal(venue.Address, result.Address);
        Assert.Equal(venue.City, result.City);
        Assert.Equal(venue.Country, result.Country);
        Assert.Equal(venue.PostalCode, result.PostalCode);
        Assert.Equal(venue.Latitude, result.Latitude);
        Assert.Equal(venue.Longitude, result.Longitude);
        Assert.Equal(venue.ContactEmail, result.ContactEmail);
        Assert.Equal(venue.ContactPhone, result.ContactPhone);
    }

    [Fact]
    public async Task UpdateAsync_ExistingVenue_PersistsUpdatedFields()
    {
        await using var db = CreateDbContext();
        var venue = SeedVenue(db);
        await db.SaveChangesAsync();
        var service = CreateService(db);
        var model = new EditVenueViewModel
        {
            Id = venue.Id,
            Name = "Updated Venue",
            Description = "Updated description",
            Address = "2 Updated Street",
            City = "Plovdiv",
            Country = "Bulgaria",
            PostalCode = "4000",
            Latitude = 42.1354f,
            Longitude = 24.7453f,
            ContactEmail = "updated@example.com",
            ContactPhone = "999999"
        };

        var result = await service.UpdateAsync(model);

        var savedVenue = await db.Venues.AsNoTracking().SingleAsync(v => v.Id == venue.Id);
        Assert.True(result);
        Assert.Equal(model.Name, savedVenue.Name);
        Assert.Equal(model.Description, savedVenue.Description);
        Assert.Equal(model.Address, savedVenue.Address);
        Assert.Equal(model.City, savedVenue.City);
        Assert.Equal(model.Country, savedVenue.Country);
        Assert.Equal(model.PostalCode, savedVenue.PostalCode);
        Assert.Equal(model.Latitude, savedVenue.Latitude);
        Assert.Equal(model.Longitude, savedVenue.Longitude);
        Assert.Equal(model.ContactEmail, savedVenue.ContactEmail);
        Assert.Equal(model.ContactPhone, savedVenue.ContactPhone);
        Assert.NotEqual(default, savedVenue.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_MissingVenue_ReturnsFalseAndDoesNotCreateRows()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var model = new EditVenueViewModel
        {
            Id = Guid.NewGuid(),
            Name = "Missing Venue",
            Address = "1 Missing Street",
            City = "Sofia",
            Country = "Bulgaria"
        };

        var result = await service.UpdateAsync(model);

        Assert.False(result);
        Assert.Empty(await db.Venues.ToListAsync());
    }

    [Fact]
    public async Task DeactivateAsync_ExistingVenue_PersistsInactiveState()
    {
        await using var db = CreateDbContext();
        var venue = SeedVenue(db, isActive: true);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.DeactivateAsync(venue.Id);

        var savedVenue = await db.Venues.AsNoTracking().SingleAsync(v => v.Id == venue.Id);
        Assert.True(result);
        Assert.False(savedVenue.IsActive);
        Assert.NotEqual(default, savedVenue.UpdatedAt);
    }

    [Fact]
    public async Task DeactivateAsync_MissingVenue_ReturnsFalseAndDoesNotCreateRows()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);

        var result = await service.DeactivateAsync(Guid.NewGuid());

        Assert.False(result);
        Assert.Empty(await db.Venues.ToListAsync());
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static VenueService CreateService(ApplicationDbContext db)
        => new(new Repository(db));

    private static AddVenueViewModel CreateAddVenueViewModel()
        => new()
        {
            Name = "Main Venue",
            Description = "Large venue",
            Address = "1 Main Street",
            City = "Sofia",
            Country = "Bulgaria",
            PostalCode = "1000",
            Latitude = 42.6767f,
            Longitude = 23.3219f,
            ContactEmail = "venue@example.com",
            ContactPhone = "1234567890"
        };

    private static Venue SeedVenue(
        ApplicationDbContext db,
        string name = "Main Venue",
        bool isActive = true)
    {
        var venue = new Venue
        {
            Id = Guid.NewGuid(),
            CreatedBy = Guid.NewGuid(),
            Name = name,
            Description = "Large venue",
            Address = "1 Main Street",
            City = "Sofia",
            Country = "Bulgaria",
            PostalCode = "1000",
            Latitude = 42.6767f,
            Longitude = 23.3219f,
            ContactEmail = "venue@example.com",
            ContactPhone = "1234567890",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Venues.Add(venue);
        return venue;
    }
}
