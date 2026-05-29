using EventHub.Core.Models.Room;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Tests.Integration.Admin;

public class RoomServiceAdminFlowIntegrationTests
{
    [Fact]
    public async Task AddRoomAsync_ValidRoom_PersistsRoom()
    {
        await using var db = CreateDbContext();
        var venue = SeedVenue(db);
        await db.SaveChangesAsync();
        var service = CreateRoomService(db);
        var userId = Guid.NewGuid();
        var model = new AddRoomViewModel
        {
            VenueId = venue.Id,
            Name = "Main Room",
            Description = "Large room",
            Capacity = 100,
            RoomType = RoomType.Auditorium,
            IsActive = true
        };

        var roomId = await service.AddRoomAsync(model, userId);

        var savedRoom = await db.Rooms.SingleAsync(r => r.RoomId == roomId);
        Assert.Equal(venue.Id, savedRoom.VenueId);
        Assert.Equal(userId, savedRoom.CreatedBy);
        Assert.Equal(model.Name, savedRoom.Name);
        Assert.Equal(model.Description, savedRoom.Description);
        Assert.Equal(model.Capacity, savedRoom.Capacity);
        Assert.Equal(model.RoomType, savedRoom.RoomType);
        Assert.True(savedRoom.IsActive);
    }

    [Fact]
    public async Task UpdateRoomAsync_ExistingRoom_PersistsUpdatedFields()
    {
        await using var db = CreateDbContext();
        var firstVenue = SeedVenue(db);
        var secondVenue = SeedVenue(db, name: "Second Venue");
        var room = SeedRoom(db, firstVenue.Id);
        await db.SaveChangesAsync();
        var service = CreateRoomService(db);
        var model = new EditRoomViewModel
        {
            Id = room.RoomId,
            VenueId = secondVenue.Id,
            Name = "Updated Room",
            Description = "Updated description",
            Capacity = 250,
            RoomType = RoomType.Arena
        };

        var result = await service.UpdateRoomAsync(model);

        var savedRoom = await db.Rooms.AsNoTracking().SingleAsync(r => r.RoomId == room.RoomId);
        Assert.True(result);
        Assert.Equal(secondVenue.Id, savedRoom.VenueId);
        Assert.Equal(model.Name, savedRoom.Name);
        Assert.Equal(model.Description, savedRoom.Description);
        Assert.Equal(model.Capacity, savedRoom.Capacity);
        Assert.Equal(model.RoomType, savedRoom.RoomType);
        Assert.NotEqual(default, savedRoom.UpdatedAt);
    }

    [Fact]
    public async Task DeactivateRoomAsync_ExistingRoom_PersistsInactiveState()
    {
        await using var db = CreateDbContext();
        var venue = SeedVenue(db);
        var room = SeedRoom(db, venue.Id, isActive: true);
        await db.SaveChangesAsync();
        var service = CreateRoomService(db);

        var result = await service.DeactivateRoomAsync(room.RoomId);

        var savedRoom = await db.Rooms.AsNoTracking().SingleAsync(r => r.RoomId == room.RoomId);
        Assert.True(result);
        Assert.False(savedRoom.IsActive);
        Assert.NotEqual(default, savedRoom.UpdatedAt);
    }

    [Fact]
    public async Task SaveLayoutAsync_NewLayout_PersistsSeatLayoutAndSeats()
    {
        await using var db = CreateDbContext();
        var venue = SeedVenue(db);
        var room = SeedRoom(db, venue.Id, capacity: 4);
        await db.SaveChangesAsync();
        var service = CreateSeatLayoutService(db);
        var userId = Guid.NewGuid();
        var request = new SaveSeatLayoutRequest
        {
            RoomId = room.RoomId,
            LayoutName = "Default Layout",
            GridRows = 2,
            GridColumns = 2,
            Seats =
            [
                new() { Row = 0, Column = 0 },
                new() { Row = 0, Column = 1 },
                new() { Row = 1, Column = 0 }
            ]
        };

        await service.SaveLayoutAsync(request, userId);

        var savedLayout = await db.SeatLayouts.SingleAsync(l => l.RoomId == room.RoomId);
        var savedSeats = await db.Seats.Where(s => s.RoomId == room.RoomId).OrderBy(s => s.SeatNumber).ToListAsync();
        Assert.Equal(userId, savedLayout.CreatedBy);
        Assert.Equal("Default Layout", savedLayout.Name);
        Assert.Equal(3, savedLayout.TotalSeats);
        Assert.True(savedLayout.IsActive);
        Assert.Equal(3, savedSeats.Count);
        Assert.Equal([1, 2, 3], savedSeats.Select(s => s.SeatNumber));
        Assert.All(savedSeats, s => Assert.True(s.IsActive));
    }

    [Fact]
    public async Task SaveLayoutAsync_OverCapacity_ThrowsAndDoesNotPersistSeats()
    {
        await using var db = CreateDbContext();
        var venue = SeedVenue(db);
        var room = SeedRoom(db, venue.Id, capacity: 1);
        await db.SaveChangesAsync();
        var service = CreateSeatLayoutService(db);
        var request = new SaveSeatLayoutRequest
        {
            RoomId = room.RoomId,
            LayoutName = "Too Large",
            GridRows = 1,
            GridColumns = 2,
            Seats =
            [
                new() { Row = 0, Column = 0 },
                new() { Row = 0, Column = 1 }
            ]
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SaveLayoutAsync(request, Guid.NewGuid()));

        Assert.Contains("exceeds room capacity", exception.Message);
        Assert.Empty(await db.Seats.Where(s => s.RoomId == room.RoomId).ToListAsync());
        Assert.Empty(await db.SeatLayouts.Where(l => l.RoomId == room.RoomId).ToListAsync());
    }

    [Fact]
    public async Task SaveLayoutAsync_RemovedSeat_PersistsInactiveSeat()
    {
        await using var db = CreateDbContext();
        var venue = SeedVenue(db);
        var room = SeedRoom(db, venue.Id, capacity: 4);
        SeedSeat(db, room.RoomId, row: 0, column: 0, seatNumber: 1);
        SeedSeat(db, room.RoomId, row: 0, column: 1, seatNumber: 2);
        await db.SaveChangesAsync();
        var service = CreateSeatLayoutService(db);
        var request = new SaveSeatLayoutRequest
        {
            RoomId = room.RoomId,
            LayoutName = "Updated Layout",
            GridRows = 1,
            GridColumns = 1,
            Seats = [new() { Row = 0, Column = 0 }]
        };

        await service.SaveLayoutAsync(request, Guid.NewGuid());

        var activeSeat = await db.Seats.SingleAsync(s => s.RoomId == room.RoomId && s.Row == 0 && s.Column == 0);
        var removedSeat = await db.Seats.SingleAsync(s => s.RoomId == room.RoomId && s.Row == 0 && s.Column == 1);
        Assert.True(activeSeat.IsActive);
        Assert.False(removedSeat.IsActive);
    }

    [Fact]
    public async Task RemoveSeatsFromZoneAsync_ClearsSeatZoneAssignmentsAndUpdatesZoneCapacity()
    {
        await using var db = CreateDbContext();
        var venue = SeedVenue(db);
        var room = SeedRoom(db, venue.Id);
        var zone = SeedZone(db, room.RoomId, capacity: 2);
        var firstSeat = SeedSeat(db, room.RoomId, row: 0, column: 0, seatNumber: 1, zoneId: zone.Id);
        var secondSeat = SeedSeat(db, room.RoomId, row: 0, column: 1, seatNumber: 2, zoneId: zone.Id);
        await db.SaveChangesAsync();
        var service = CreateSeatLayoutService(db);
        var request = new RemoveFromZoneRequest
        {
            RoomId = room.RoomId,
            SeatIds = [firstSeat.Id]
        };

        await service.RemoveSeatsFromZoneAsync(request);

        var removedSeat = await db.Seats.SingleAsync(s => s.Id == firstSeat.Id);
        var remainingSeat = await db.Seats.SingleAsync(s => s.Id == secondSeat.Id);
        var savedZone = await db.Zones.SingleAsync(z => z.Id == zone.Id);
        Assert.Null(removedSeat.ZoneId);
        Assert.Equal(zone.Id, remainingSeat.ZoneId);
        Assert.Equal(1, savedZone.Capacity);
    }

    [Fact]
    public async Task DeleteZoneAsync_ClearsSeatAssignmentsAndPersistsInactiveZone()
    {
        await using var db = CreateDbContext();
        var venue = SeedVenue(db);
        var room = SeedRoom(db, venue.Id);
        var zone = SeedZone(db, room.RoomId, capacity: 1);
        var seat = SeedSeat(db, room.RoomId, row: 0, column: 0, seatNumber: 1, zoneId: zone.Id);
        await db.SaveChangesAsync();
        var service = CreateSeatLayoutService(db);

        await service.DeleteZoneAsync(zone.Id);

        var savedSeat = await db.Seats.SingleAsync(s => s.Id == seat.Id);
        var savedZone = await db.Zones.SingleAsync(z => z.Id == zone.Id);
        Assert.Null(savedSeat.ZoneId);
        Assert.False(savedZone.IsActive);
        Assert.NotEqual(default, savedZone.UpdatedAt);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static RoomService CreateRoomService(ApplicationDbContext db)
        => new(new Repository(db));

    private static SeatLayoutService CreateSeatLayoutService(ApplicationDbContext db)
        => new(new Repository(db));

    private static Venue SeedVenue(ApplicationDbContext db, string name = "Main Venue")
    {
        var venue = new Venue
        {
            Id = Guid.NewGuid(),
            CreatedBy = Guid.NewGuid(),
            Name = name,
            Address = "1 Main Street",
            City = "Sofia",
            Country = "Bulgaria",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Venues.Add(venue);
        return venue;
    }

    private static Room SeedRoom(
        ApplicationDbContext db,
        Guid venueId,
        long capacity = 100,
        bool isActive = true)
    {
        var room = new Room
        {
            RoomId = Guid.NewGuid(),
            VenueId = venueId,
            CreatedBy = Guid.NewGuid(),
            Name = "Main Room",
            Description = "Room description",
            Capacity = capacity,
            RoomType = RoomType.Auditorium,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Rooms.Add(room);
        return room;
    }

    private static Seat SeedSeat(
        ApplicationDbContext db,
        Guid roomId,
        int row,
        int column,
        int seatNumber,
        Guid? zoneId = null)
    {
        var seat = new Seat
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            ZoneId = zoneId,
            Row = row,
            Column = column,
            PositionX = column,
            PositionY = row,
            SeatNumber = seatNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Seats.Add(seat);
        return seat;
    }

    private static Zone SeedZone(ApplicationDbContext db, Guid roomId, int capacity = 0)
    {
        var zone = new Zone
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            CreatedBy = Guid.NewGuid(),
            Name = "VIP",
            ZoneType = ZoneType.VIP,
            Capacity = capacity,
            DisplayOrder = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Zones.Add(zone);
        return zone;
    }
}
