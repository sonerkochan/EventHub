using EventHub.Core.Contracts;
using EventHub.Core.Models.Payment;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;

namespace EventHub.Tests.Unit.Services;

// NOTE: CreateCheckoutSessionAsync, CreateSeatCheckoutSessionAsync (happy path) and
// HandleWebhookAsync call the Stripe network API / static EventUtility and are not
// unit-testable here — they belong in Integration/E2E coverage. These tests cover the
// network-free logic: the empty-lines guard and the payment-history query/mapping.
[Trait("Category", "Unit")]
public class StripePaymentServiceTests
{
    [Fact]
    public async Task CreateSeatCheckoutSessionAsync_WithNoLines_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IRepository>();
        var service = CreateService(repo);

        var request = new CreateSeatCheckoutRequest
        {
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Lines = []
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateSeatCheckoutSessionAsync(request));

        // Guard must short-circuit before persisting anything.
        repo.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Never);
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetPaymentHistoryAsync_ExcludesPendingAndOtherUsers_OrdersByCreatedAtDesc()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var accepted = new Payment { Id = Guid.NewGuid(), UserId = userId, Status = Payment.PaymentStatus.Accepted, Amount = 100, Currency = "EUR", CreatedAt = new DateTime(2026, 5, 1) };
        var newer = new Payment { Id = Guid.NewGuid(), UserId = userId, Status = Payment.PaymentStatus.Accepted, Amount = 50, Currency = "EUR", CreatedAt = new DateTime(2026, 5, 9) };
        var pending = new Payment { Id = Guid.NewGuid(), UserId = userId, Status = Payment.PaymentStatus.Pending, Amount = 10, Currency = "EUR", CreatedAt = new DateTime(2026, 5, 5) };
        var otherUser = new Payment { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Status = Payment.PaymentStatus.Accepted, Amount = 999, Currency = "EUR", CreatedAt = new DateTime(2026, 5, 5) };

        var ticket1 = new Ticket { Id = Guid.NewGuid(), EventId = eventId };
        var ticket2 = new Ticket { Id = Guid.NewGuid(), EventId = eventId };
        var ticket3 = new Ticket { Id = Guid.NewGuid(), EventId = eventId };

        var paymentTickets = new[]
        {
            new PaymentTicket { PaymentId = accepted.Id, TicketId = ticket1.Id },
            new PaymentTicket { PaymentId = newer.Id, TicketId = ticket2.Id },
            new PaymentTicket { PaymentId = pending.Id, TicketId = ticket3.Id },
            new PaymentTicket { PaymentId = otherUser.Id, TicketId = ticket1.Id }
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<Payment>()).Returns(new[] { accepted, newer, pending, otherUser }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<PaymentTicket>()).Returns(paymentTickets.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Ticket>()).Returns(new[] { ticket1, ticket2, ticket3 }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Event>()).Returns(new[] { new Event { Id = eventId, EventName = "Gala" } }.AsQueryable().BuildMock());

        var service = CreateService(repo);

        var result = (await service.GetPaymentHistoryAsync(userId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(newer.Id, result[0].Id);     // newest first
        Assert.Equal(accepted.Id, result[1].Id);
        Assert.All(result, p => Assert.Equal("Gala", p.EventName));
        Assert.Equal("Accepted", result[0].Status);
    }

    [Fact]
    public async Task GetPaymentHistoryAsync_PaymentWithMultipleTickets_ReturnedOnce()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var payment = new Payment { Id = Guid.NewGuid(), UserId = userId, Status = Payment.PaymentStatus.Accepted, Amount = 200, Currency = "EUR", CreatedAt = new DateTime(2026, 5, 1) };

        var ticketA = new Ticket { Id = Guid.NewGuid(), EventId = eventId };
        var ticketB = new Ticket { Id = Guid.NewGuid(), EventId = eventId };
        var paymentTickets = new[]
        {
            new PaymentTicket { PaymentId = payment.Id, TicketId = ticketA.Id },
            new PaymentTicket { PaymentId = payment.Id, TicketId = ticketB.Id }
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<Payment>()).Returns(new[] { payment }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<PaymentTicket>()).Returns(paymentTickets.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Ticket>()).Returns(new[] { ticketA, ticketB }.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Event>()).Returns(new[] { new Event { Id = eventId, EventName = "Gala" } }.AsQueryable().BuildMock());

        var service = CreateService(repo);

        var result = (await service.GetPaymentHistoryAsync(userId)).ToList();

        Assert.Single(result);
        Assert.Equal(payment.Id, result[0].Id);
    }

    private static StripePaymentService CreateService(Mock<IRepository> repo)
    {
        var ticketService = new Mock<ITicketService>();
        var options = Options.Create(new StripeOptions
        {
            SecretKey = "sk_test_dummy",
            PublishableKey = "pk_test_dummy",
            WebhookSecret = "whsec_test_dummy"
        });

        return new StripePaymentService(repo.Object, ticketService.Object, options);
    }
}
