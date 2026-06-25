using EventHub.Core.Contracts;
using EventHub.Core.Models.Payment;
using EventHub.Core.Models.Refund;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using MockQueryable.Moq;
using Moq;

using DataEvent = EventHub.Infrastructure.Data.Models.Event;
using DataRefund = EventHub.Infrastructure.Data.Models.Refund;
using RefundStatus = EventHub.Infrastructure.Data.Models.Refund.RefundStatus;

namespace EventHub.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class RefundServiceTests
{
    [Fact]
    public async Task RequestAsync_CreatesPendingRefund_ReturnsNewId()
    {
        var paymentId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();
        DataRefund? added = null;

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<DataRefund>()))
            .Callback<DataRefund>(r => added = r)
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new RefundService(repo.Object);

        var id = await service.RequestAsync(new CreateRefundViewModel
        {
            PaymentId = paymentId,
            Amount = 42.50f,
            Reason = "Event cancelled"
        }, requestedBy);

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(added);
        Assert.Equal(paymentId, added!.PaymentId);
        Assert.Equal(requestedBy, added.RequestedBy);
        Assert.Equal(RefundStatus.Pending, added.Status);
        Assert.Equal("EUR", added.Currency);
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RequestTicketRefundAsync_WhenEligible_CreatesPendingRefundWithSeventyPercentAmount()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            UserId = userId,
            EventId = eventId,
            Status = TicketStatus.Purchased,
            Price = 100f,
            Currency = "EUR"
        };

        var ev = new DataEvent
        {
            Id = eventId,
            OrganizerId = Guid.NewGuid(),
            IsActive = true,
            AllowRefunds = true,
            StartDateTime = DateTime.UtcNow.AddDays(5)
        };

        var payment = new Payment
        {
            Id = paymentId,
            Status = Payment.PaymentStatus.Accepted,
            StripePaymentIntentId = "pi_test_123",
            Currency = "EUR"
        };

        DataRefund? added = null;
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<Ticket>()).Returns(new[] { ticket }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<DataEvent>()).Returns(new[] { ev }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<DataRefund>()).Returns(Array.Empty<DataRefund>().AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<PaymentTicket>()).Returns(new[] { new PaymentTicket { PaymentId = paymentId, TicketId = ticketId } }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Payment>()).Returns(new[] { payment }.AsQueryable().BuildMock());
        repo.Setup(r => r.AddAsync(It.IsAny<DataRefund>()))
            .Callback<DataRefund>(refund => added = refund)
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new RefundService(repo.Object);

        var result = await service.RequestTicketRefundAsync(ticketId, userId, "Plans changed");

        Assert.True(result.Success);
        Assert.Equal(70f, result.RefundAmount);
        Assert.NotNull(added);
        Assert.Equal(ticketId, added!.TicketId);
        Assert.Equal(paymentId, added.PaymentId);
        Assert.Equal(RefundStatus.Pending, added.Status);
        Assert.Equal(70f, added.Amount);
        Assert.Equal("Plans changed", added.Reason);
    }

    [Fact]
    public async Task RequestTicketRefundAsync_WhenEventStartsWithinFortyEightHours_Fails()
    {
        var userId = Guid.NewGuid();
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventId = Guid.NewGuid(),
            Status = TicketStatus.Purchased,
            Price = 100f
        };
        var ev = new DataEvent
        {
            Id = ticket.EventId,
            IsActive = true,
            AllowRefunds = true,
            StartDateTime = DateTime.UtcNow.AddHours(24)
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<Ticket>()).Returns(new[] { ticket }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<DataEvent>()).Returns(new[] { ev }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<DataRefund>()).Returns(Array.Empty<DataRefund>().AsQueryable().BuildMock());

        var service = new RefundService(repo.Object);

        var result = await service.RequestTicketRefundAsync(ticket.Id, userId, null);

        Assert.False(result.Success);
        Assert.Equal("Messages.Refund.TooLate", result.ErrorMessage);
    }

    [Fact]
    public async Task RequestTicketRefundAsync_WhenDuplicateRefundExists_Fails()
    {
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket
        {
            Id = ticketId,
            UserId = userId,
            EventId = Guid.NewGuid(),
            Status = TicketStatus.Purchased,
            Price = 100f
        };
        var ev = new DataEvent
        {
            Id = ticket.EventId,
            IsActive = true,
            AllowRefunds = true,
            StartDateTime = DateTime.UtcNow.AddDays(4)
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<Ticket>()).Returns(new[] { ticket }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<DataEvent>()).Returns(new[] { ev }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<DataRefund>()).Returns(new[] { new DataRefund { Id = Guid.NewGuid(), TicketId = ticketId } }.AsQueryable().BuildMock());

        var service = new RefundService(repo.Object);

        var result = await service.RequestTicketRefundAsync(ticketId, userId, null);

        Assert.False(result.Success);
        Assert.Equal("Messages.Refund.Duplicate", result.ErrorMessage);
    }

    [Fact]
    public async Task RequestTicketRefundAsync_WhenPaymentIntentMissing_Fails()
    {
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var ticket = new Ticket
        {
            Id = ticketId,
            UserId = userId,
            EventId = Guid.NewGuid(),
            Status = TicketStatus.Purchased,
            Price = 100f
        };
        var ev = new DataEvent
        {
            Id = ticket.EventId,
            IsActive = true,
            AllowRefunds = true,
            StartDateTime = DateTime.UtcNow.AddDays(4)
        };
        var payment = new Payment
        {
            Id = paymentId,
            Status = Payment.PaymentStatus.Accepted
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<Ticket>()).Returns(new[] { ticket }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<DataEvent>()).Returns(new[] { ev }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<DataRefund>()).Returns(Array.Empty<DataRefund>().AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<PaymentTicket>()).Returns(new[] { new PaymentTicket { PaymentId = paymentId, TicketId = ticketId } }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Payment>()).Returns(new[] { payment }.AsQueryable().BuildMock());

        var service = new RefundService(repo.Object);

        var result = await service.RequestTicketRefundAsync(ticketId, userId, null);

        Assert.False(result.Success);
        Assert.Equal("Messages.Refund.PaymentIntentMissing", result.ErrorMessage);
    }

    [Fact]
    public async Task ApproveTicketRefundAsync_WhenOwnerAndStripeSucceeds_CompletesRefundAndMarksPaymentRefunded()
    {
        var organizerId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        var refund = new DataRefund
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            PaymentId = paymentId,
            RequestedBy = buyerId,
            Amount = 70f,
            Status = RefundStatus.Pending
        };
        var ticket = new Ticket
        {
            Id = ticketId,
            EventId = eventId,
            UserId = buyerId,
            Status = TicketStatus.Purchased,
            Price = 100f,
            PricingTierId = tierId
        };
        var ev = new DataEvent
        {
            Id = eventId,
            OrganizerId = organizerId,
            TicketsSold = 3,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var payment = new Payment
        {
            Id = paymentId,
            Status = Payment.PaymentStatus.Accepted,
            StripePaymentIntentId = "pi_123"
        };
        var tier = new EventPricingTier
        {
            Id = tierId,
            SoldQuantity = 2
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<DataRefund>()).Returns(new[] { refund }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<Ticket>()).Returns(new[] { ticket }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<DataEvent>()).Returns(new[] { ev }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<Payment>()).Returns(new[] { payment }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<EventPricingTier>()).Returns(new[] { tier }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<PaymentTicket>()).Returns(new[] { new PaymentTicket { PaymentId = paymentId, TicketId = ticketId } }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Ticket>()).Returns(new[] { ticket }.AsQueryable().BuildMock());
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var refundProcessor = new Mock<IPaymentRefundProcessor>();
        refundProcessor
            .Setup(p => p.RefundPaymentIntentAsync(It.IsAny<PaymentRefundRequest>()))
            .ReturnsAsync(PaymentRefundResult.Succeeded("re_123"));

        var service = new RefundService(repo.Object, refundProcessor.Object);

        var result = await service.ApproveTicketRefundAsync(refund.Id, organizerId);

        Assert.True(result.Success);
        Assert.Equal(RefundStatus.Completed, refund.Status);
        Assert.Equal("re_123", refund.StripeRefundId);
        Assert.Equal(TicketStatus.Refunded, ticket.Status);
        Assert.Equal(2, ev.TicketsSold);
        Assert.Equal(1, tier.SoldQuantity);
        Assert.Equal(Payment.PaymentStatus.Refunded, payment.Status);
        refundProcessor.Verify(
            p => p.RefundPaymentIntentAsync(It.Is<PaymentRefundRequest>(r =>
                r.PaymentIntentId == "pi_123" &&
                r.AmountMinor == 7000 &&
                r.Metadata["ticketId"] == ticketId.ToString())),
            Times.Once);
    }

    [Fact]
    public async Task ApproveTicketRefundAsync_WhenOrganizerDoesNotOwnEvent_Fails()
    {
        var refund = new DataRefund
        {
            Id = Guid.NewGuid(),
            TicketId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            Status = RefundStatus.Pending
        };
        var ticket = new Ticket
        {
            Id = refund.TicketId.Value,
            EventId = Guid.NewGuid(),
            Status = TicketStatus.Purchased
        };
        var ev = new DataEvent
        {
            Id = ticket.EventId,
            OrganizerId = Guid.NewGuid()
        };
        var payment = new Payment
        {
            Id = refund.PaymentId,
            Status = Payment.PaymentStatus.Accepted,
            StripePaymentIntentId = "pi_123"
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<DataRefund>()).Returns(new[] { refund }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<Ticket>()).Returns(new[] { ticket }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<DataEvent>()).Returns(new[] { ev }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<Payment>()).Returns(new[] { payment }.AsQueryable().BuildMock());

        var refundProcessor = new Mock<IPaymentRefundProcessor>();
        var service = new RefundService(repo.Object, refundProcessor.Object);

        var result = await service.ApproveTicketRefundAsync(refund.Id, Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("Messages.Refund.UnauthorizedOrganizer", result.ErrorMessage);
        refundProcessor.Verify(p => p.RefundPaymentIntentAsync(It.IsAny<PaymentRefundRequest>()), Times.Never);
    }

    [Fact]
    public async Task ApproveTicketRefundAsync_WhenStripeFails_LeavesRefundPending()
    {
        var organizerId = Guid.NewGuid();
        var refund = new DataRefund
        {
            Id = Guid.NewGuid(),
            TicketId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            Status = RefundStatus.Pending,
            Amount = 70f
        };
        var ticket = new Ticket
        {
            Id = refund.TicketId.Value,
            EventId = Guid.NewGuid(),
            Status = TicketStatus.Purchased
        };
        var ev = new DataEvent
        {
            Id = ticket.EventId,
            OrganizerId = organizerId,
            TicketsSold = 4
        };
        var payment = new Payment
        {
            Id = refund.PaymentId,
            Status = Payment.PaymentStatus.Accepted,
            StripePaymentIntentId = "pi_123"
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<DataRefund>()).Returns(new[] { refund }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<Ticket>()).Returns(new[] { ticket }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<DataEvent>()).Returns(new[] { ev }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<Payment>()).Returns(new[] { payment }.AsQueryable().BuildMock());

        var refundProcessor = new Mock<IPaymentRefundProcessor>();
        refundProcessor
            .Setup(p => p.RefundPaymentIntentAsync(It.IsAny<PaymentRefundRequest>()))
            .ReturnsAsync(PaymentRefundResult.Failed("Messages.Refund.StripeFailed"));

        var service = new RefundService(repo.Object, refundProcessor.Object);

        var result = await service.ApproveTicketRefundAsync(refund.Id, organizerId);

        Assert.False(result.Success);
        Assert.Equal(RefundStatus.Pending, refund.Status);
        Assert.Equal(TicketStatus.Purchased, ticket.Status);
        Assert.Equal(Payment.PaymentStatus.Accepted, payment.Status);
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ApproveTicketRefundAsync_WhenOtherPaymentTicketsRemain_DoesNotMarkPaymentRefunded()
    {
        var organizerId = Guid.NewGuid();
        var currentTicketId = Guid.NewGuid();
        var otherTicketId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var refund = new DataRefund
        {
            Id = Guid.NewGuid(),
            TicketId = currentTicketId,
            PaymentId = paymentId,
            Status = RefundStatus.Pending,
            Amount = 35f
        };
        var currentTicket = new Ticket
        {
            Id = currentTicketId,
            EventId = Guid.NewGuid(),
            Status = TicketStatus.Purchased
        };
        var otherTicket = new Ticket
        {
            Id = otherTicketId,
            EventId = currentTicket.EventId,
            Status = TicketStatus.Purchased
        };
        var ev = new DataEvent
        {
            Id = currentTicket.EventId,
            OrganizerId = organizerId,
            TicketsSold = 2
        };
        var payment = new Payment
        {
            Id = paymentId,
            Status = Payment.PaymentStatus.Accepted,
            StripePaymentIntentId = "pi_123"
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<DataRefund>()).Returns(new[] { refund }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<Ticket>()).Returns(new[] { currentTicket, otherTicket }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<DataEvent>()).Returns(new[] { ev }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<Payment>()).Returns(new[] { payment }.AsQueryable().BuildMock());
        repo.Setup(r => r.All<EventPricingTier>()).Returns(Array.Empty<EventPricingTier>().AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<PaymentTicket>()).Returns(new[]
        {
            new PaymentTicket { PaymentId = paymentId, TicketId = currentTicketId },
            new PaymentTicket { PaymentId = paymentId, TicketId = otherTicketId }
        }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Ticket>()).Returns(new[] { currentTicket, otherTicket }.AsQueryable().BuildMock());
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var refundProcessor = new Mock<IPaymentRefundProcessor>();
        refundProcessor
            .Setup(p => p.RefundPaymentIntentAsync(It.IsAny<PaymentRefundRequest>()))
            .ReturnsAsync(PaymentRefundResult.Succeeded("re_123"));

        var service = new RefundService(repo.Object, refundProcessor.Object);

        var result = await service.ApproveTicketRefundAsync(refund.Id, organizerId);

        Assert.True(result.Success);
        Assert.Equal(Payment.PaymentStatus.Accepted, payment.Status);
    }

    [Fact]
    public async Task RejectTicketRefundAsync_WhenOwner_SetsRejectedAndStoresComment()
    {
        var organizerId = Guid.NewGuid();
        var refund = new DataRefund
        {
            Id = Guid.NewGuid(),
            TicketId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            Status = RefundStatus.Pending,
            Amount = 70f
        };
        var ticket = new Ticket
        {
            Id = refund.TicketId.Value,
            EventId = Guid.NewGuid(),
            Status = TicketStatus.Purchased
        };
        var ev = new DataEvent
        {
            Id = ticket.EventId,
            OrganizerId = organizerId
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<DataRefund>()).Returns(new[] { refund }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Ticket>()).Returns(new[] { ticket }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<DataEvent>()).Returns(new[] { ev }.AsQueryable().BuildMock());
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new RefundService(repo.Object);

        var result = await service.RejectTicketRefundAsync(refund.Id, organizerId, "Event policy");

        Assert.True(result.Success);
        Assert.Equal(RefundStatus.Rejected, refund.Status);
        Assert.Equal("Event policy", refund.ProcessorComment);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyUsersRefunds_OrderedByCreatedAtDesc()
    {
        var userId = Guid.NewGuid();
        var refunds = new[]
        {
            new DataRefund { Id = Guid.NewGuid(), RequestedBy = userId, CreatedAt = new DateTime(2026, 5, 1) },
            new DataRefund { Id = Guid.NewGuid(), RequestedBy = userId, CreatedAt = new DateTime(2026, 5, 3) },
            new DataRefund { Id = Guid.NewGuid(), RequestedBy = Guid.NewGuid(), CreatedAt = new DateTime(2026, 5, 5) }
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<DataRefund>()).Returns(refunds.AsQueryable().BuildMock());

        var service = new RefundService(repo.Object);

        var result = (await service.GetByUserAsync(userId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(userId, r.RequestedBy));
        Assert.True(result[0].CreatedAt > result[1].CreatedAt);
    }

    [Fact]
    public async Task ApproveAsync_WhenPending_TransitionsToApproved()
    {
        var refund = new DataRefund { Id = Guid.NewGuid(), Status = RefundStatus.Pending };
        var processedBy = Guid.NewGuid();
        var repo = CreateRepoWith(refund);

        var service = new RefundService(repo.Object);

        var result = await service.ApproveAsync(refund.Id, processedBy);

        Assert.True(result);
        Assert.Equal(RefundStatus.Approved, refund.Status);
        Assert.Equal(processedBy, refund.ProcessedBy);
    }

    [Fact]
    public async Task CompleteAsync_WhenApproved_TransitionsToCompleted()
    {
        var refund = new DataRefund { Id = Guid.NewGuid(), Status = RefundStatus.Approved };
        var repo = CreateRepoWith(refund);

        var service = new RefundService(repo.Object);

        Assert.True(await service.CompleteAsync(refund.Id));
        Assert.Equal(RefundStatus.Completed, refund.Status);
    }

    [Fact]
    public async Task GetByIdAsync_MapsDetailFields()
    {
        var refund = new DataRefund
        {
            Id = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            TicketId = Guid.NewGuid(),
            RequestedBy = Guid.NewGuid(),
            ProcessedBy = Guid.NewGuid(),
            Amount = 15.5f,
            Currency = "EUR",
            Reason = "Duplicate charge",
            ProcessorComment = "Handled",
            Status = RefundStatus.Approved
        };
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<DataRefund>()).Returns(new[] { refund }.AsQueryable().BuildMock());

        var service = new RefundService(repo.Object);

        var result = await service.GetByIdAsync(refund.Id);

        Assert.NotNull(result);
        Assert.Equal(refund.Id, result!.Id);
        Assert.Equal(refund.TicketId, result.TicketId);
        Assert.Equal("Handled", result.ProcessorComment);
    }

    private static Mock<IRepository> CreateRepoWith(DataRefund refund)
    {
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<DataRefund>()).Returns(new[] { refund }.AsQueryable().BuildMock());
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        return repo;
    }
}
