using EventHub.Core.Contracts;
using EventHub.Core.Models.Payment;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DataEvent = EventHub.Infrastructure.Data.Models.Event; // ← Alias to avoid conflict

namespace EventHub.Core.Services
{
    public class StripePaymentService : IPaymentService
    {
        private readonly IRepository repo;
        private readonly ITicketService ticketService;
        private readonly StripeOptions options;

        public StripePaymentService(
            IRepository _repo,
            ITicketService _ticketService,
            IOptions<StripeOptions> _options)
        {
            repo = _repo;
            ticketService = _ticketService;
            options = _options.Value;

            StripeConfiguration.ApiKey = options.SecretKey;
        }

        public async Task<string> CreateCheckoutSessionAsync(CreateCheckoutRequest request)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                TicketId = Guid.Empty,
                Amount = (float)(request.UnitPrice * request.Quantity),
                Currency = request.Currency.ToUpperInvariant(),
                Status = Payment.PaymentStatus.Pending,
                Method = Payment.PaymentMethod.Card,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(payment);
            await repo.SaveChangesAsync();

            var sessionOptions = new SessionCreateOptions
            {
                PaymentMethodTypes = ["card"],
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = request.Currency,
                            UnitAmountDecimal = request.UnitPrice * 100,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = request.EventName,
                                Description = $"{request.Quantity} ticket(s)"
                            }
                        },
                        Quantity = request.Quantity
                    }
                ],
                Mode = "payment",
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    ["internalPaymentId"] = payment.Id.ToString(),
                    ["eventId"] = request.EventId.ToString(),
                    ["userId"] = request.UserId.ToString(),
                    ["quantity"] = request.Quantity.ToString()
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(sessionOptions);

            payment.StripeSessionId = session.Id;
            payment.UpdatedAt = DateTime.UtcNow;
            repo.Update(payment);
            await repo.SaveChangesAsync();

            return session.Url;
        }

        public async Task HandleWebhookAsync(string payload, string stripeSignature)
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                stripeSignature,
                options.WebhookSecret,
                throwOnApiVersionMismatch: false);

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                if (session == null) return;

                await FulfillOrderAsync(session);
            }
            else if (stripeEvent.Type == EventTypes.CheckoutSessionExpired)
            {
                var session = stripeEvent.Data.Object as Session;
                if (session == null) return;

                await CancelPendingPaymentAsync(session.Metadata["internalPaymentId"]);
            }
        }

        public async Task<IEnumerable<PaymentListViewModel>> GetPaymentHistoryAsync(Guid userId)
        {
            var raw = await repo.AllReadonly<Payment>()
                .Where(p => p.UserId == userId && p.Status != Payment.PaymentStatus.Pending)
                .Join(repo.AllReadonly<PaymentTicket>(),
                      p => p.Id,
                      pt => pt.PaymentId,
                      (p, pt) => new { p, pt })
                .Join(repo.AllReadonly<Ticket>(),
                      ppt => ppt.pt.TicketId,
                      t => t.Id,
                      (ppt, t) => new { ppt.p, t })
                .Join(repo.AllReadonly<DataEvent>(),
                      ptt => ptt.t.EventId,
                      e => e.Id,
                      (ptt, e) => new
                      {
                          ptt.p.Id,
                          EventName = e.EventName!,
                          ptt.p.Amount,
                          Currency = ptt.p.Currency ?? "EUR",
                          ptt.p.Status,          // ← bring the enum, convert after
                          ptt.p.CreatedAt
                      })
                .ToListAsync(); // ← materialize here, then do client-side ops below

            return raw
                .DistinctBy(p => p.Id)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PaymentListViewModel
                {
                    Id = p.Id,
                    EventName = p.EventName,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status.ToString(), // ← safe, runs in-memory now
                    CreatedAt = p.CreatedAt
                });
        }

        private async Task FulfillOrderAsync(Session session)
        {
            if (!session.Metadata.TryGetValue("internalPaymentId", out var paymentIdStr)
                || !Guid.TryParse(paymentIdStr, out var paymentId))
                return;

            var payment = await repo.All<Payment>()
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null || payment.Status != Payment.PaymentStatus.Pending)
                return;

            if (!Guid.TryParse(session.Metadata["eventId"], out var eventId)
                || !Guid.TryParse(session.Metadata["userId"], out var userId)
                || !int.TryParse(session.Metadata["quantity"], out var quantity))
                return;

            var ticketIds = await ticketService.PurchaseAsync(eventId, userId, quantity);
            if (ticketIds.Count == 0) return;

            foreach (var ticketId in ticketIds)
            {
                await repo.AddAsync(new PaymentTicket
                {
                    PaymentId = payment.Id,
                    TicketId = ticketId
                });
            }

            payment.TicketId = ticketIds[0];
            payment.StripePaymentIntentId = session.PaymentIntentId;
            payment.Status = Payment.PaymentStatus.Accepted;
            payment.SucceededAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            repo.Update(payment);
            await repo.SaveChangesAsync();
        }

        private async Task CancelPendingPaymentAsync(string? paymentIdStr)
        {
            if (!Guid.TryParse(paymentIdStr, out var paymentId)) return;

            var payment = await repo.All<Payment>()
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null || payment.Status != Payment.PaymentStatus.Pending) return;

            payment.Status = Payment.PaymentStatus.Declined;
            payment.FailedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            repo.Update(payment);
            await repo.SaveChangesAsync();
        }
    }
}