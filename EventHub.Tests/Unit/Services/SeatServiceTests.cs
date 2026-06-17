using EventHub.Core.Models.Seat;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using MockQueryable.Moq;
using Moq;

namespace EventHub.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class SeatServiceTests
{
    [Fact]
    public async Task CreateAsync_AddsActiveSeatAndSaves_ReturnsNewId()
    {
        var roomId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        Seat? added = null;

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Seat>()))
            .Callback<Seat>(s => added = s)
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new SeatService(repo.Object);

        var model = new CreateSeatViewModel
        {
            RoomId = roomId,
            ZoneId = zoneId,
            SeatNumber = 5,
            Row = 2,
            Column = 3
        };

        var id = await service.CreateAsync(model);

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(added);
        Assert.Equal(id, added!.Id);
        Assert.Equal(roomId, added.RoomId);
        Assert.Equal(zoneId, added.ZoneId);
        Assert.True(added.IsActive);
        repo.Verify(r => r.AddAsync(It.IsAny<Seat>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateBatchAsync_NoExistingSeats_NumbersStartAtOne()
    {
        var roomId = Guid.NewGuid();
        List<Seat> captured = [];

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<Seat>())
            .Returns(Array.Empty<Seat>().AsQueryable().BuildMock());
        repo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Seat>>()))
            .Callback<IEnumerable<Seat>>(s => captured = s.ToList())
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new SeatService(repo.Object);

        var ids = (await service.CreateBatchAsync(roomId, 3, null)).ToList();

        Assert.Equal(3, ids.Count);
        Assert.Equal(3, captured.Count);
        Assert.Equal([1, 2, 3], captured.Select(s => s.SeatNumber));
        Assert.All(captured, s => Assert.Equal(roomId, s.RoomId));
        Assert.All(captured, s => Assert.True(s.IsActive));
    }

    [Fact]
    public async Task CreateBatchAsync_WithExistingSeats_ContinuesFromHighestSeatNumber()
    {
        var roomId = Guid.NewGuid();
        var existing = new[]
        {
            new Seat { Id = Guid.NewGuid(), RoomId = roomId, SeatNumber = 5 },
            new Seat { Id = Guid.NewGuid(), RoomId = roomId, SeatNumber = 2 }
        };
        List<Seat> captured = [];

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<Seat>())
            .Returns(existing.AsQueryable().BuildMock());
        repo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Seat>>()))
            .Callback<IEnumerable<Seat>>(s => captured = s.ToList())
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new SeatService(repo.Object);

        await service.CreateBatchAsync(roomId, 2, null);

        Assert.Equal([6, 7], captured.Select(s => s.SeatNumber));
    }

    [Fact]
    public async Task GetByRoomAsync_ReturnsActiveSeatsWithZoneName_OrderedByRowThenColumn()
    {
        var roomId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var seats = new[]
        {
            new Seat { Id = Guid.NewGuid(), RoomId = roomId, ZoneId = zoneId, SeatNumber = 1, Row = 2, Column = 1, IsActive = true },
            new Seat { Id = Guid.NewGuid(), RoomId = roomId, ZoneId = null, SeatNumber = 2, Row = 1, Column = 5, IsActive = true },
            new Seat { Id = Guid.NewGuid(), RoomId = roomId, ZoneId = zoneId, SeatNumber = 3, Row = 9, Column = 9, IsActive = false },
            new Seat { Id = Guid.NewGuid(), RoomId = Guid.NewGuid(), ZoneId = zoneId, SeatNumber = 4, Row = 1, Column = 1, IsActive = true }
        };
        var zones = new[] { new Zone { Id = zoneId, Name = "VIP" } };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<Seat>()).Returns(seats.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Zone>()).Returns(zones.AsQueryable().BuildMock());

        var service = new SeatService(repo.Object);

        var result = (await service.GetByRoomAsync(roomId)).ToList();

        Assert.Equal(2, result.Count);
        // Row 1 (no zone) comes before Row 2 (VIP)
        Assert.Equal(2, result[0].SeatNumber);
        Assert.Null(result[0].ZoneName);
        Assert.Equal(1, result[1].SeatNumber);
        Assert.Equal("VIP", result[1].ZoneName);
    }

    [Fact]
    public async Task GetForEditAsync_WhenSeatMissing_ReturnsNull()
    {
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<Seat>(It.IsAny<object>())).ReturnsAsync((Seat)null!);

        var service = new SeatService(repo.Object);

        Assert.Null(await service.GetForEditAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_WhenSeatMissing_ReturnsFalseWithoutSaving()
    {
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<Seat>(It.IsAny<object>())).ReturnsAsync((Seat)null!);

        var service = new SeatService(repo.Object);

        var result = await service.UpdateAsync(new EditSeatViewModel { Id = Guid.NewGuid() });

        Assert.False(result);
        repo.Verify(r => r.Update(It.IsAny<Seat>()), Times.Never);
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenSeatExists_UpdatesFieldsAndSaves()
    {
        var seat = new Seat { Id = Guid.NewGuid(), RoomId = Guid.NewGuid(), SeatNumber = 1, Row = 1, Column = 1 };
        var newRoom = Guid.NewGuid();
        var newZone = Guid.NewGuid();

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<Seat>(seat.Id)).ReturnsAsync(seat);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new SeatService(repo.Object);

        var result = await service.UpdateAsync(new EditSeatViewModel
        {
            Id = seat.Id,
            RoomId = newRoom,
            ZoneId = newZone,
            SeatNumber = 42,
            Row = 7,
            Column = 8
        });

        Assert.True(result);
        Assert.Equal(newRoom, seat.RoomId);
        Assert.Equal(newZone, seat.ZoneId);
        Assert.Equal(42, seat.SeatNumber);
        repo.Verify(r => r.Update(seat), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_WhenSeatExists_SetsInactiveAndSaves()
    {
        var seat = new Seat { Id = Guid.NewGuid(), IsActive = true };
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<Seat>(seat.Id)).ReturnsAsync(seat);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new SeatService(repo.Object);

        Assert.True(await service.DeactivateAsync(seat.Id));
        Assert.False(seat.IsActive);
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AssignToZoneAsync_WhenSeatExists_SetsZoneAndSaves()
    {
        var seat = new Seat { Id = Guid.NewGuid(), ZoneId = null };
        var zoneId = Guid.NewGuid();
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<Seat>(seat.Id)).ReturnsAsync(seat);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new SeatService(repo.Object);

        Assert.True(await service.AssignToZoneAsync(seat.Id, zoneId));
        Assert.Equal(zoneId, seat.ZoneId);
        repo.Verify(r => r.Update(seat), Times.Once);
    }

    [Fact]
    public async Task RemoveFromZoneAsync_WhenSeatExists_ClearsZoneAndSaves()
    {
        var seat = new Seat { Id = Guid.NewGuid(), ZoneId = Guid.NewGuid() };
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<Seat>(seat.Id)).ReturnsAsync(seat);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new SeatService(repo.Object);

        Assert.True(await service.RemoveFromZoneAsync(seat.Id));
        Assert.Null(seat.ZoneId);
        repo.Verify(r => r.Update(seat), Times.Once);
    }
}
