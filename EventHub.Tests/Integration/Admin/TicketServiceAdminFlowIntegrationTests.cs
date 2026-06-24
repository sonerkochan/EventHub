using EventHub.Core.Models.Admin;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EventHub.Tests.Integration.Admin;

[Trait("Category", "Integration")]
public class TicketServiceAdminFlowIntegrationTests
{
    [Fact]
    public async Task AdminUpdateTicketAsync_ExistingTicketStatusToUsed_PersistsUsedStatus()
    {
        await using var db = CreateDbContext();
        var roomId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        SeedEvent(db, eventId, roomId);
        SeedSeat(db, seatId, roomId, seatNumber: 1);
        var ticket = SeedTicket(db, eventId, seatId, TicketStatus.Purchased);
        await db.SaveChangesAsync();
        var service = CreateService(db);
        var request = new AdminTicketEditRequest
        {
            TicketId = ticket.Id,
            SeatId = seatId,
            Status = TicketStatus.Used
        };

        var result = await service.AdminUpdateTicketAsync(request);

        var savedTicket = await db.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.Equal(TicketStatus.Used, savedTicket.Status);
        Assert.True(savedTicket.IsUsed);
        Assert.NotEqual(default, savedTicket.ValidatedAt);
        Assert.NotEqual(default, savedTicket.PurchasedAt);
    }

    [Fact]
    public async Task AdminUpdateTicketAsync_MoveToAvailableSeat_PersistsSeatChange()
    {
        await using var db = CreateDbContext();
        var roomId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var oldSeatId = Guid.NewGuid();
        var newSeatId = Guid.NewGuid();
        SeedEvent(db, eventId, roomId);
        SeedSeat(db, oldSeatId, roomId, seatNumber: 1);
        SeedSeat(db, newSeatId, roomId, seatNumber: 2);
        var ticket = SeedTicket(db, eventId, oldSeatId, TicketStatus.Purchased);
        await db.SaveChangesAsync();
        var service = CreateService(db);
        var request = new AdminTicketEditRequest
        {
            TicketId = ticket.Id,
            SeatId = newSeatId,
            Status = TicketStatus.Purchased
        };

        var result = await service.AdminUpdateTicketAsync(request);

        var savedTicket = await db.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.Equal(newSeatId, savedTicket.SeatId);
    }

    [Fact]
    public async Task AdminUpdateTicketAsync_MoveToTakenSeat_ReturnsFalseAndDoesNotChangeSeat()
    {
        await using var db = CreateDbContext();
        var roomId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var currentSeatId = Guid.NewGuid();
        var takenSeatId = Guid.NewGuid();
        SeedEvent(db, eventId, roomId);
        SeedSeat(db, currentSeatId, roomId, seatNumber: 1);
        SeedSeat(db, takenSeatId, roomId, seatNumber: 2);
        var ticket = SeedTicket(db, eventId, currentSeatId, TicketStatus.Purchased);
        SeedTicket(db, eventId, takenSeatId, TicketStatus.Purchased, ticketNumber: 2002);
        await db.SaveChangesAsync();
        var service = CreateService(db);
        var request = new AdminTicketEditRequest
        {
            TicketId = ticket.Id,
            SeatId = takenSeatId,
            Status = TicketStatus.Purchased
        };

        var result = await service.AdminUpdateTicketAsync(request);

        var savedTicket = await db.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        Assert.False(result.Success);
        Assert.Equal("That seat is already taken.", result.Error);
        Assert.Equal(currentSeatId, savedTicket.SeatId);
    }

    [Fact]
    public async Task AdminRefundTicketAsync_PurchasedTicket_PersistsRefundedTicketPaymentAndTierCount()
    {
        await using var db = CreateDbContext();
        var roomId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        SeedEvent(db, eventId, roomId);
        SeedSeat(db, seatId, roomId, seatNumber: 1);

        var tier = new EventPricingTier
        {
            Id = tierId,
            EventId = eventId,
            ZoneId = Guid.NewGuid(),
            TierName = "VIP",
            Price = 50,
            Currency = "EUR",
            AvailableQuantity = 10,
            SoldQuantity = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.EventPricingTiers.Add(tier);

        var ticket = SeedTicket(db, eventId, seatId, TicketStatus.Purchased, pricingTierId: tierId);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = ticket.UserId,
            TicketId = ticket.Id,
            Amount = ticket.Price,
            Currency = "EUR",
            Status = Payment.PaymentStatus.Accepted,
            Method = Payment.PaymentMethod.Card,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Payments.Add(payment);
        db.PaymentTickets.Add(new PaymentTicket
        {
            PaymentId = payment.Id,
            TicketId = ticket.Id
        });

        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();

        var service = CreateService(db);

        var result = await service.AdminRefundTicketAsync(ticket.Id, processedBy: Guid.NewGuid());

        var savedTicket = await db.Tickets
            .AsNoTracking()
            .SingleAsync(t => t.Id == ticket.Id);

        var savedPayment = await db.Payments
            .AsNoTracking()
            .SingleAsync(p => p.Id == payment.Id);

        var savedTier = await db.EventPricingTiers
            .AsNoTracking()
            .SingleAsync(t => t.Id == tierId);

        Assert.True(result);
        Assert.Equal(TicketStatus.Refunded, savedTicket.Status);
        Assert.Equal(Payment.PaymentStatus.Refunded, savedPayment.Status);
        Assert.NotEqual(default, savedPayment.RefundedAt);
        Assert.Equal(2, savedTier.SoldQuantity);
    }

    [Fact]
    public async Task AdminRefundTicketAsync_AlreadyRefundedTicket_ReturnsFalseAndDoesNotChangePayment()
    {
        await using var db = CreateDbContext();
        var roomId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        SeedEvent(db, eventId, roomId);
        SeedSeat(db, seatId, roomId, seatNumber: 1);
        var ticket = SeedTicket(db, eventId, seatId, TicketStatus.Refunded);
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = ticket.UserId,
            TicketId = ticket.Id,
            Amount = ticket.Price,
            Currency = "EUR",
            Status = Payment.PaymentStatus.Accepted,
            Method = Payment.PaymentMethod.Card,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Payments.Add(payment);
        db.PaymentTickets.Add(new PaymentTicket { PaymentId = payment.Id, TicketId = ticket.Id });
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.AdminRefundTicketAsync(ticket.Id, processedBy: Guid.NewGuid());

        var savedTicket = await db.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        var savedPayment = await db.Payments.AsNoTracking().SingleAsync(p => p.Id == payment.Id);
        Assert.False(result);
        Assert.Equal(TicketStatus.Refunded, savedTicket.Status);
        Assert.Equal(Payment.PaymentStatus.Accepted, savedPayment.Status);
        Assert.Equal(default, savedPayment.RefundedAt);
    }

    [Fact]
    public async Task AdminRefundTicketAsync_MissingTicket_ReturnsFalseAndDoesNotChangeDatabase()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);

        var result = await service.AdminRefundTicketAsync(Guid.NewGuid(), processedBy: Guid.NewGuid());

        Assert.False(result);
        Assert.Empty(await db.Tickets.ToListAsync());
        Assert.Empty(await db.Payments.ToListAsync());
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static TicketService CreateService(ApplicationDbContext db)
    {
        var qrCodeService = new Mock<IQRCodeService>();
        qrCodeService
            .Setup(s => s.GenerateQRCode(It.IsAny<string>(), It.IsAny<int>()))
            .Returns("qr-code");

        return new TicketService(
            new Repository(db),
            qrCodeService.Object,
            new HttpContextAccessor());
    }

    private static void SeedEvent(ApplicationDbContext db, Guid eventId, Guid roomId)
        => db.Events.Add(new Event
        {
            Id = eventId,
            RoomId = roomId,
            OrganizerId = Guid.NewGuid(),
            EventName = "Concert",
            EventType = EventType.Concert,
            EventStatus = EventStatus.Published,
            EventPriority = EventPriority.Normal,
            StartDateTime = DateTime.UtcNow.AddDays(10),
            EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(2),
            TotalTickets = 100,
            TicketsSold = 1,
            BasePrice = 25,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

    private static void SeedSeat(ApplicationDbContext db, Guid seatId, Guid roomId, int seatNumber)
        => db.Seats.Add(new Seat
        {
            Id = seatId,
            RoomId = roomId,
            SeatNumber = seatNumber,
            Row = 0,
            Column = seatNumber - 1,
            PositionX = seatNumber - 1,
            PositionY = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

    private static Ticket SeedTicket(
        ApplicationDbContext db,
        Guid eventId,
        Guid seatId,
        TicketStatus status,
        long ticketNumber = 1001,
        Guid? pricingTierId = null)
    {
        var now = DateTime.UtcNow;
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = Guid.NewGuid(),
            SeatId = seatId,
            PricingTierId = pricingTierId ?? Guid.Empty,
            ValidatedBy = Guid.Empty,
            TicketNumber = ticketNumber,
            Status = status,
            Price = 25,
            Currency = "EUR",
            HashedCode = $"HASH-{ticketNumber}",
            IsUsed = status == TicketStatus.Used,
            ReservedAt = now.AddMinutes(-30),
            ReservationExpiresAt = now.AddMinutes(30),
            PurchasedAt = status is TicketStatus.Purchased or TicketStatus.Used or TicketStatus.Refunded
                ? now.AddMinutes(-20)
                : default,
            ValidatedAt = status == TicketStatus.Used ? now.AddMinutes(-10) : default
        };

        db.Tickets.Add(ticket);
        return ticket;
    }
}
