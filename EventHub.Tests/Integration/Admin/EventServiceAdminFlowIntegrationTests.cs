using EventHub.Core.Contracts;
using EventHub.Core.Models.Currency;
using EventHub.Core.Models.Event;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using DataEvent = EventHub.Infrastructure.Data.Models.Event;

namespace EventHub.Tests.Integration.Admin;

public class EventServiceAdminFlowIntegrationTests
{
    [Fact]
    public async Task CreateAsync_ValidEvent_PersistsDraftActiveEvent()
    {
        await using var db = CreateDbContext();
        var room = SeedRoom(db);
        await db.SaveChangesAsync();
        var service = CreateService(db);
        var organizerId = Guid.NewGuid();
        var refundDeadline = DateTime.UtcNow.AddDays(3);
        var coverPhotoId = Guid.NewGuid();
        var model = new CreateEventViewModel
        {
            EventName = "Admin Created Event",
            Description = "Created from admin flow",
            EventType = EventType.Conference,
            EventPriority = EventPriority.Paid,
            RoomId = room.RoomId,
            StartDateTime = DateTime.UtcNow.AddDays(10),
            EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(2),
            TotalTickets = 120,
            BasePrice = 25.50m,
            AllowRefunds = true,
            RefundDeadline = refundDeadline,
            CoverImageUrl = "/images/events/admin-created.jpg",
            CoverPhotoId = coverPhotoId,
            Address = "  1 Event Street  ",
            City = "  Sofia  ",
            CountryCode = " bg ",
            Latitude = 42.6977m,
            Longitude = 23.3219m
        };

        var eventId = await service.CreateAsync(model, organizerId);

        var savedEvent = await db.Events.SingleAsync(e => e.Id == eventId);
        Assert.NotEqual(Guid.Empty, eventId);
        Assert.Equal(organizerId, savedEvent.OrganizerId);
        Assert.Equal(room.RoomId, savedEvent.RoomId);
        Assert.Equal(model.EventName, savedEvent.EventName);
        Assert.Equal(model.Description, savedEvent.Description);
        Assert.Equal(EventStatus.Draft, savedEvent.EventStatus);
        Assert.Equal(model.EventType, savedEvent.EventType);
        Assert.Equal(model.EventPriority, savedEvent.EventPriority);
        Assert.Equal(model.StartDateTime, savedEvent.StartDateTime);
        Assert.Equal(model.EndDateTime, savedEvent.EndDateTime);
        Assert.Equal(model.TotalTickets, savedEvent.TotalTickets);
        Assert.Equal(0, savedEvent.TicketsSold);
        Assert.Equal(model.BasePrice, savedEvent.BasePrice);
        Assert.True(savedEvent.AllowRefunds);
        Assert.Equal(refundDeadline, savedEvent.RefundDeadline);
        Assert.True(savedEvent.IsActive);
        Assert.Equal(model.CoverImageUrl, savedEvent.CoverImageUrl);
        Assert.Equal(coverPhotoId, savedEvent.CoverPhotoId);
        Assert.Equal("1 Event Street", savedEvent.Address);
        Assert.Equal("Sofia", savedEvent.City);
        Assert.Equal("BG", savedEvent.CountryCode);
        Assert.Equal(model.Latitude, savedEvent.Latitude);
        Assert.Equal(model.Longitude, savedEvent.Longitude);
        Assert.NotEqual(default, savedEvent.CreatedAt);
        Assert.NotEqual(default, savedEvent.UpdatedAt);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsOnlyActiveEventsFromDatabase()
    {
        await using var db = CreateDbContext();
        var room = SeedRoom(db);
        var activeEvent = SeedEvent(db, room.RoomId, "Active Event", isActive: true);
        SeedEvent(db, room.RoomId, "Inactive Event", isActive: false);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = (await service.GetAllEventsAsync()).ToList();

        Assert.Single(result);
        Assert.Equal(activeEvent.Id, result[0].Id);
        Assert.Equal(activeEvent.EventName, result[0].EventName);
        Assert.Equal(room.Name, result[0].RoomName);
        Assert.True(result[0].IsActive);
        Assert.Equal(activeEvent.BasePrice, result[0].PriceAmount);
        Assert.Equal("EUR", result[0].DisplayCurrency);
    }

    [Fact]
    public async Task GetPublishedEventsAsync_ReturnsOnlyActivePublishedEventsFromDatabase()
    {
        await using var db = CreateDbContext();
        var room = SeedRoom(db);
        var publishedEvent = SeedEvent(
            db,
            room.RoomId,
            "Published Event",
            EventStatus.Published,
            isActive: true);
        SeedEvent(db, room.RoomId, "Draft Event", EventStatus.Draft, isActive: true);
        SeedEvent(db, room.RoomId, "Inactive Published Event", EventStatus.Published, isActive: false);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = (await service.GetPublishedEventsAsync()).ToList();

        Assert.Single(result);
        Assert.Equal(publishedEvent.Id, result[0].Id);
        Assert.Equal(EventStatus.Published, result[0].EventStatus);
        Assert.True(result[0].IsActive);
    }

    [Fact]
    public async Task GetEventForEditAsync_ExistingEvent_ReturnsEditModelFromDatabase()
    {
        await using var db = CreateDbContext();
        var room = SeedRoom(db);
        var eventEntity = SeedEvent(db, room.RoomId);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.GetEventForEditAsync(eventEntity.Id);

        Assert.NotNull(result);
        Assert.Equal(eventEntity.Id, result.Id);
        Assert.Equal(eventEntity.EventName, result.EventName);
        Assert.Equal(eventEntity.Description, result.Description);
        Assert.Equal(eventEntity.EventType, result.EventType);
        Assert.Equal(eventEntity.EventStatus, result.EventStatus);
        Assert.Equal(eventEntity.EventPriority, result.EventPriority);
        Assert.Equal(eventEntity.RoomId, result.RoomId);
        Assert.Equal(eventEntity.StartDateTime, result.StartDateTime);
        Assert.Equal(eventEntity.EndDateTime, result.EndDateTime);
        Assert.Equal(eventEntity.TotalTickets, result.TotalTickets);
        Assert.Equal(eventEntity.BasePrice, result.BasePrice);
        Assert.Equal(eventEntity.AllowRefunds, result.AllowRefunds);
        Assert.Equal(eventEntity.RefundDeadline, result.RefundDeadline);
        Assert.Equal(eventEntity.CoverImageUrl, result.CoverImageUrl);
        Assert.Equal(eventEntity.CoverPhotoId, result.CoverPhotoId);
        Assert.Equal(eventEntity.Address, result.Address);
        Assert.Equal(eventEntity.City, result.City);
        Assert.Equal(eventEntity.CountryCode, result.CountryCode);
        Assert.Equal(eventEntity.Latitude, result.Latitude);
        Assert.Equal(eventEntity.Longitude, result.Longitude);
    }

    [Fact]
    public async Task UpdateAsync_ExistingEvent_PersistsUpdatedFields()
    {
        await using var db = CreateDbContext();
        var originalRoom = SeedRoom(db, name: "Original Room");
        var updatedRoom = SeedRoom(db, name: "Updated Room");
        var eventEntity = SeedEvent(db, originalRoom.RoomId);
        await db.SaveChangesAsync();
        var service = CreateService(db);
        var updatedCoverPhotoId = Guid.NewGuid();
        var model = new EditEventViewModel
        {
            Id = eventEntity.Id,
            EventName = "Updated Event",
            Description = "Updated description",
            EventType = EventType.Workshop,
            EventStatus = EventStatus.Published,
            EventPriority = EventPriority.GoodReputation,
            RoomId = updatedRoom.RoomId,
            StartDateTime = DateTime.UtcNow.AddDays(20),
            EndDateTime = DateTime.UtcNow.AddDays(20).AddHours(4),
            TotalTickets = 80,
            BasePrice = 35.75m,
            AllowRefunds = false,
            RefundDeadline = null,
            CoverImageUrl = "/images/events/updated.jpg",
            CoverPhotoId = updatedCoverPhotoId,
            Address = "  2 Updated Street  ",
            City = "  Plovdiv  ",
            CountryCode = " bg ",
            Latitude = 42.1354m,
            Longitude = 24.7453m
        };

        var result = await service.UpdateAsync(model);

        var savedEvent = await db.Events.AsNoTracking().SingleAsync(e => e.Id == eventEntity.Id);
        Assert.True(result);
        Assert.Equal(model.EventName, savedEvent.EventName);
        Assert.Equal(model.Description, savedEvent.Description);
        Assert.Equal(model.EventType, savedEvent.EventType);
        Assert.Equal(model.EventStatus, savedEvent.EventStatus);
        Assert.Equal(model.EventPriority, savedEvent.EventPriority);
        Assert.Equal(updatedRoom.RoomId, savedEvent.RoomId);
        Assert.Equal(model.StartDateTime, savedEvent.StartDateTime);
        Assert.Equal(model.EndDateTime, savedEvent.EndDateTime);
        Assert.Equal(model.TotalTickets, savedEvent.TotalTickets);
        Assert.Equal(model.BasePrice, savedEvent.BasePrice);
        Assert.False(savedEvent.AllowRefunds);
        Assert.Equal(default, savedEvent.RefundDeadline);
        Assert.Equal(model.CoverImageUrl, savedEvent.CoverImageUrl);
        Assert.Equal(updatedCoverPhotoId, savedEvent.CoverPhotoId);
        Assert.Equal("2 Updated Street", savedEvent.Address);
        Assert.Equal("Plovdiv", savedEvent.City);
        Assert.Equal("BG", savedEvent.CountryCode);
        Assert.Equal(model.Latitude, savedEvent.Latitude);
        Assert.Equal(model.Longitude, savedEvent.Longitude);
        Assert.NotEqual(default, savedEvent.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_MissingEvent_ReturnsFalseAndDoesNotCreateRows()
    {
        await using var db = CreateDbContext();
        var room = SeedRoom(db);
        await db.SaveChangesAsync();
        var service = CreateService(db);
        var model = new EditEventViewModel
        {
            Id = Guid.NewGuid(),
            EventName = "Missing Event",
            EventType = EventType.Other,
            EventStatus = EventStatus.Draft,
            EventPriority = EventPriority.Normal,
            RoomId = room.RoomId,
            StartDateTime = DateTime.UtcNow.AddDays(2),
            EndDateTime = DateTime.UtcNow.AddDays(2).AddHours(1),
            TotalTickets = 10
        };

        var result = await service.UpdateAsync(model);

        Assert.False(result);
        Assert.Empty(await db.Events.ToListAsync());
    }

    [Fact]
    public async Task PublishAsync_ExistingEvent_PersistsPublishedStatus()
    {
        await using var db = CreateDbContext();
        var room = SeedRoom(db);
        var eventEntity = SeedEvent(db, room.RoomId, status: EventStatus.Draft);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.PublishAsync(eventEntity.Id);

        var savedEvent = await db.Events.AsNoTracking().SingleAsync(e => e.Id == eventEntity.Id);
        Assert.True(result);
        Assert.Equal(EventStatus.Published, savedEvent.EventStatus);
        Assert.True(savedEvent.IsActive);
        Assert.NotEqual(default, savedEvent.UpdatedAt);
    }

    [Fact]
    public async Task PublishAsync_MissingEvent_ReturnsFalseAndDoesNotCreateRows()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);

        var result = await service.PublishAsync(Guid.NewGuid());

        Assert.False(result);
        Assert.Empty(await db.Events.ToListAsync());
    }

    [Fact]
    public async Task DeactivateAsync_ExistingEvent_PersistsInactiveCancelledState()
    {
        await using var db = CreateDbContext();
        var room = SeedRoom(db);
        var eventEntity = SeedEvent(db, room.RoomId, status: EventStatus.Published, isActive: true);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.DeactivateAsync(eventEntity.Id);

        var savedEvent = await db.Events.AsNoTracking().SingleAsync(e => e.Id == eventEntity.Id);
        Assert.True(result);
        Assert.False(savedEvent.IsActive);
        Assert.Equal(EventStatus.Cancelled, savedEvent.EventStatus);
        Assert.NotEqual(default, savedEvent.UpdatedAt);
    }

    [Fact]
    public async Task DeactivateAsync_MissingEvent_ReturnsFalseAndDoesNotCreateRows()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);

        var result = await service.DeactivateAsync(Guid.NewGuid());

        Assert.False(result);
        Assert.Empty(await db.Events.ToListAsync());
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static EventService CreateService(ApplicationDbContext db)
    {
        var currencyDisplayService = new Mock<ICurrencyDisplayService>();
        currencyDisplayService
            .Setup(s => s.FormatAsync(It.IsAny<decimal>(), It.IsAny<string?>()))
            .ReturnsAsync((decimal amount, string? _) => new CurrencyDisplayValue
            {
                Amount = amount,
                Currency = "EUR",
                Text = $"{amount:0.00} EUR"
            });

        return new EventService(
            new Repository(db),
            new MemoryCache(new MemoryCacheOptions()),
            currencyDisplayService.Object);
    }

    private static Venue SeedVenue(ApplicationDbContext db)
    {
        var venue = new Venue
        {
            Id = Guid.NewGuid(),
            CreatedBy = Guid.NewGuid(),
            Name = "Main Venue",
            Description = "Large venue",
            Address = "1 Main Street",
            City = "Sofia",
            Country = "Bulgaria",
            PostalCode = "1000",
            Latitude = 42.6767f,
            Longitude = 23.3219f,
            ContactEmail = "venue@example.com",
            ContactPhone = "1234567890",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Venues.Add(venue);
        return venue;
    }

    private static Room SeedRoom(ApplicationDbContext db, string name = "Main Room")
    {
        var venue = SeedVenue(db);
        var room = new Room
        {
            RoomId = Guid.NewGuid(),
            VenueId = venue.Id,
            CreatedBy = Guid.NewGuid(),
            Name = name,
            Description = "Room for admin event tests",
            Capacity = 150,
            RoomType = RoomType.Auditorium,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Rooms.Add(room);
        return room;
    }

    private static DataEvent SeedEvent(
        ApplicationDbContext db,
        Guid roomId,
        string eventName = "Seeded Event",
        EventStatus status = EventStatus.Draft,
        bool isActive = true)
    {
        var eventEntity = new DataEvent
        {
            Id = Guid.NewGuid(),
            OrganizerId = Guid.NewGuid(),
            RoomId = roomId,
            EventName = eventName,
            Description = "Seeded event description",
            EventType = EventType.Concert,
            EventStatus = status,
            EventPriority = EventPriority.Normal,
            StartDateTime = DateTime.UtcNow.AddDays(5),
            EndDateTime = DateTime.UtcNow.AddDays(5).AddHours(2),
            TotalTickets = 100,
            TicketsSold = 5,
            BasePrice = 20m,
            AllowRefunds = true,
            RefundDeadline = DateTime.UtcNow.AddDays(3),
            IsActive = isActive,
            CoverImageUrl = "/images/events/seeded.jpg",
            CoverPhotoId = Guid.NewGuid(),
            Address = "1 Seed Street",
            City = "Sofia",
            CountryCode = "BG",
            Latitude = 42.6977m,
            Longitude = 23.3219m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Events.Add(eventEntity);
        return eventEntity;
    }
}
