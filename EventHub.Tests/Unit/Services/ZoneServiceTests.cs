using EventHub.Core.Models.Zone;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using MockQueryable.Moq;
using Moq;

namespace EventHub.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class ZoneServiceTests
{
    [Fact]
    public async Task CreateAsync_AddsActiveZoneWithCreator_ReturnsNewId()
    {
        var roomId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        Zone? added = null;

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Zone>()))
            .Callback<Zone>(z => added = z)
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new ZoneService(repo.Object);

        var model = new CreateZoneViewModel
        {
            RoomId = roomId,
            Name = "Balcony",
            ZoneType = ZoneType.VIP,
            Capacity = 50,
            DisplayOrder = 2
        };

        var id = await service.CreateAsync(model, createdBy);

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(added);
        Assert.Equal(roomId, added!.RoomId);
        Assert.Equal(createdBy, added.CreatedBy);
        Assert.Equal("Balcony", added.Name);
        Assert.Equal(ZoneType.VIP, added.ZoneType);
        Assert.True(added.IsActive);
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByRoomAsync_ReturnsActiveZonesWithRoomName_OrderedByDisplayOrder()
    {
        var roomId = Guid.NewGuid();
        var zones = new[]
        {
            new Zone { Id = Guid.NewGuid(), RoomId = roomId, Name = "Second", DisplayOrder = 2, IsActive = true },
            new Zone { Id = Guid.NewGuid(), RoomId = roomId, Name = "First", DisplayOrder = 1, IsActive = true },
            new Zone { Id = Guid.NewGuid(), RoomId = roomId, Name = "Hidden", DisplayOrder = 0, IsActive = false },
            new Zone { Id = Guid.NewGuid(), RoomId = Guid.NewGuid(), Name = "OtherRoom", DisplayOrder = 0, IsActive = true }
        };
        var rooms = new[] { new Room { RoomId = roomId, Name = "Main Hall" } };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<Zone>()).Returns(zones.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Room>()).Returns(rooms.AsQueryable().BuildMock());

        var service = new ZoneService(repo.Object);

        var result = (await service.GetByRoomAsync(roomId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("First", result[0].Name);
        Assert.Equal("Second", result[1].Name);
        Assert.All(result, z => Assert.Equal("Main Hall", z.RoomName));
    }

    [Fact]
    public async Task GetForEditAsync_WhenZoneMissing_ReturnsNull()
    {
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<Zone>(It.IsAny<object>())).ReturnsAsync((Zone)null!);

        var service = new ZoneService(repo.Object);

        Assert.Null(await service.GetForEditAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_WhenZoneMissing_ReturnsFalseWithoutSaving()
    {
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<Zone>(It.IsAny<object>())).ReturnsAsync((Zone)null!);

        var service = new ZoneService(repo.Object);

        var result = await service.UpdateAsync(new EditZoneViewModel { Id = Guid.NewGuid(), Name = "x" });

        Assert.False(result);
        repo.Verify(r => r.Update(It.IsAny<Zone>()), Times.Never);
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenZoneExists_UpdatesFieldsAndSaves()
    {
        var zone = new Zone { Id = Guid.NewGuid(), Name = "Old", Capacity = 10, DisplayOrder = 1 };
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<Zone>(zone.Id)).ReturnsAsync(zone);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new ZoneService(repo.Object);

        var result = await service.UpdateAsync(new EditZoneViewModel
        {
            Id = zone.Id,
            RoomId = zone.RoomId,
            Name = "New",
            ZoneType = ZoneType.Economy,
            Capacity = 99,
            DisplayOrder = 5
        });

        Assert.True(result);
        Assert.Equal("New", zone.Name);
        Assert.Equal(ZoneType.Economy, zone.ZoneType);
        Assert.Equal(99, zone.Capacity);
        Assert.Equal(5, zone.DisplayOrder);
        repo.Verify(r => r.Update(zone), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_WhenZoneExists_SetsInactiveAndSaves()
    {
        var zone = new Zone { Id = Guid.NewGuid(), IsActive = true };
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<Zone>(zone.Id)).ReturnsAsync(zone);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new ZoneService(repo.Object);

        Assert.True(await service.DeactivateAsync(zone.Id));
        Assert.False(zone.IsActive);
        repo.Verify(r => r.Update(zone), Times.Once);
    }

    [Fact]
    public async Task ActivateAsync_WhenZoneMissing_ReturnsFalse()
    {
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<Zone>(It.IsAny<object>())).ReturnsAsync((Zone)null!);

        var service = new ZoneService(repo.Object);

        Assert.False(await service.ActivateAsync(Guid.NewGuid()));
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
