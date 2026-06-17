using EventHub.Core.Models.Admin;
using EventHub.Core.Models.EventPricingTier;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using MockQueryable.Moq;
using Moq;

using DataPricingTier = EventHub.Infrastructure.Data.Models.EventPricingTier;

namespace EventHub.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class EventPricingTierServiceTests
{
    [Fact]
    public async Task CreateAsync_AddsActiveTierWithZeroSold_ReturnsNewId()
    {
        var eventId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        DataPricingTier? added = null;

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<DataPricingTier>()))
            .Callback<DataPricingTier>(t => added = t)
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new EventPricingTierService(repo.Object);

        var id = await service.CreateAsync(new CreatePricingTierViewModel
        {
            EventId = eventId,
            ZoneId = zoneId,
            TierName = "Early Bird",
            Price = 49.99f,
            Currency = "EUR",
            AvailableQuantity = 100
        });

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(added);
        Assert.Equal(eventId, added!.EventId);
        Assert.Equal(0, added.SoldQuantity);
        Assert.True(added.IsActive);
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByEventAsync_ReturnsActiveTiersWithZoneName_OrderedByPrice()
    {
        var eventId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var tiers = new[]
        {
            new DataPricingTier { Id = Guid.NewGuid(), EventId = eventId, ZoneId = zoneId, Price = 100, IsActive = true },
            new DataPricingTier { Id = Guid.NewGuid(), EventId = eventId, ZoneId = Guid.Empty, Price = 50, IsActive = true },
            new DataPricingTier { Id = Guid.NewGuid(), EventId = eventId, ZoneId = zoneId, Price = 1, IsActive = false },
            new DataPricingTier { Id = Guid.NewGuid(), EventId = Guid.NewGuid(), ZoneId = zoneId, Price = 1, IsActive = true }
        };
        var events = new[] { new Event { Id = eventId, EventName = "Concert" } };
        var zones = new[] { new Zone { Id = zoneId, Name = "Front Row" } };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<DataPricingTier>()).Returns(tiers.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Event>()).Returns(events.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Zone>()).Returns(zones.AsQueryable().BuildMock());

        var service = new EventPricingTierService(repo.Object);

        var result = (await service.GetByEventAsync(eventId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(50, result[0].Price);   // cheapest first
        Assert.Null(result[0].ZoneName);     // ZoneId empty -> left join yields null
        Assert.Equal(100, result[1].Price);
        Assert.Equal("Front Row", result[1].ZoneName);
        Assert.All(result, t => Assert.Equal("Concert", t.EventName));
    }

    [Fact]
    public async Task UpdateAsync_WhenTierMissing_ReturnsFalseWithoutSaving()
    {
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<DataPricingTier>(It.IsAny<object>())).ReturnsAsync((DataPricingTier)null!);

        var service = new EventPricingTierService(repo.Object);

        var result = await service.UpdateAsync(new EditPricingTierViewModel { Id = Guid.NewGuid(), TierName = "x" });

        Assert.False(result);
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeactivateAsync_WhenTierExists_SetsInactiveAndSaves()
    {
        var tier = new DataPricingTier { Id = Guid.NewGuid(), IsActive = true };
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<DataPricingTier>(tier.Id)).ReturnsAsync(tier);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new EventPricingTierService(repo.Object);

        Assert.True(await service.DeactivateAsync(tier.Id));
        Assert.False(tier.IsActive);
        repo.Verify(r => r.Update(tier), Times.Once);
    }

    [Fact]
    public async Task SetForZoneAsync_WhenTierExists_UpdatesPriceAndClampsAvailableQuantity()
    {
        var eventId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var existing = new DataPricingTier
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            ZoneId = zoneId,
            Price = 10,
            SoldQuantity = 8,
            AvailableQuantity = 20,
            IsActive = true
        };
        // 3 active seats live in the zone, but 8 already sold -> AvailableQuantity must not drop below sold.
        var seats = new[]
        {
            new Seat { Id = Guid.NewGuid(), ZoneId = zoneId, IsActive = true },
            new Seat { Id = Guid.NewGuid(), ZoneId = zoneId, IsActive = true },
            new Seat { Id = Guid.NewGuid(), ZoneId = zoneId, IsActive = true }
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<Seat>()).Returns(seats.AsQueryable().BuildMock());
        repo.Setup(r => r.All<DataPricingTier>()).Returns(new[] { existing }.AsQueryable().BuildMock());
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new EventPricingTierService(repo.Object);

        var id = await service.SetForZoneAsync(new SetZonePriceRequest
        {
            EventId = eventId,
            ZoneId = zoneId,
            Price = 25,
            Currency = "USD"
        });

        Assert.Equal(existing.Id, id);
        Assert.Equal(25, existing.Price);
        Assert.Equal("USD", existing.Currency);
        Assert.Equal(8, existing.AvailableQuantity); // max(sold=8, liveSeats=3)
        repo.Verify(r => r.Update(existing), Times.Once);
        repo.Verify(r => r.AddAsync(It.IsAny<DataPricingTier>()), Times.Never);
    }

    [Fact]
    public async Task SetForZoneAsync_WhenNoTierExists_CreatesTierNamedAfterZone()
    {
        var eventId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var zone = new Zone { Id = zoneId, Name = "Balcony", ZoneType = ZoneType.VIP };
        DataPricingTier? added = null;

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<Seat>()).Returns(Array.Empty<Seat>().AsQueryable().BuildMock());
        repo.Setup(r => r.All<DataPricingTier>()).Returns(Array.Empty<DataPricingTier>().AsQueryable().BuildMock());
        repo.Setup(r => r.GetByIdAsync<Zone>(zoneId)).ReturnsAsync(zone);
        repo.Setup(r => r.AddAsync(It.IsAny<DataPricingTier>()))
            .Callback<DataPricingTier>(t => added = t)
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new EventPricingTierService(repo.Object);

        var id = await service.SetForZoneAsync(new SetZonePriceRequest
        {
            EventId = eventId,
            ZoneId = zoneId,
            Price = 30,
            Currency = "EUR"
        });

        Assert.NotNull(added);
        Assert.Equal(added!.Id, id);
        Assert.Equal("Balcony (VIP)", added.TierName);
        Assert.Equal(1, added.AvailableQuantity); // max(1, liveSeats=0)
        Assert.True(added.IsActive);
    }

    [Fact]
    public async Task SetForZoneAsync_WhenNoTierAndZoneMissing_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<Seat>()).Returns(Array.Empty<Seat>().AsQueryable().BuildMock());
        repo.Setup(r => r.All<DataPricingTier>()).Returns(Array.Empty<DataPricingTier>().AsQueryable().BuildMock());
        repo.Setup(r => r.GetByIdAsync<Zone>(It.IsAny<object>())).ReturnsAsync((Zone)null!);

        var service = new EventPricingTierService(repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SetForZoneAsync(new SetZonePriceRequest
            {
                EventId = Guid.NewGuid(),
                ZoneId = Guid.NewGuid(),
                Price = 10
            }));
    }

    [Fact]
    public async Task RemoveForZoneAsync_WhenActiveTierExists_DeactivatesAndSaves()
    {
        var eventId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var tier = new DataPricingTier { Id = Guid.NewGuid(), EventId = eventId, ZoneId = zoneId, IsActive = true };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<DataPricingTier>()).Returns(new[] { tier }.AsQueryable().BuildMock());
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new EventPricingTierService(repo.Object);

        Assert.True(await service.RemoveForZoneAsync(eventId, zoneId));
        Assert.False(tier.IsActive);
        repo.Verify(r => r.Update(tier), Times.Once);
    }

    [Fact]
    public async Task RemoveForZoneAsync_WhenNoActiveTier_ReturnsFalse()
    {
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<DataPricingTier>()).Returns(Array.Empty<DataPricingTier>().AsQueryable().BuildMock());

        var service = new EventPricingTierService(repo.Object);

        Assert.False(await service.RemoveForZoneAsync(Guid.NewGuid(), Guid.NewGuid()));
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
