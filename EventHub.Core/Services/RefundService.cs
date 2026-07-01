using EventHub.Core.Contracts;
using EventHub.Core.Models.Payment;
using EventHub.Core.Models.Refund;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

using DataRefund = EventHub.Infrastructure.Data.Models.Refund;
using RefundStatus = EventHub.Infrastructure.Data.Models.Refund.RefundStatus;

namespace EventHub.Core.Services
{
    public class RefundService : IRefundService
    {
        private const float RefundRate = 0.70f;

        private readonly IRepository repo;
        private readonly IPaymentRefundProcessor? paymentRefundProcessor;

        public RefundService(IRepository repo, IPaymentRefundProcessor? paymentRefundProcessor = null)
        {
            this.repo = repo;
            this.paymentRefundProcessor = paymentRefundProcessor;
        }

        public async Task<Guid> RequestAsync(CreateRefundViewModel model, Guid requestedBy)
        {
            var entity = new DataRefund
            {
                Id = Guid.NewGuid(),
                PaymentId = model.PaymentId,
                RequestedBy = requestedBy,
                ProcessedBy = Guid.Empty,
                StripeRefundId = null,
                Amount = model.Amount,
                Currency = "EUR",
                Reason = model.Reason,
                Status = RefundStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<IEnumerable<RefundListViewModel>> GetAllAsync()
        {
            var refunds = await repo.AllReadonly<DataRefund>()
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return refunds.Select(MapListItem);
        }

        public async Task<IEnumerable<RefundListViewModel>> GetByUserAsync(Guid userId)
        {
            var refunds = await repo.AllReadonly<DataRefund>()
                .Where(r => r.RequestedBy == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return refunds.Select(MapListItem);
        }

        public Task<IEnumerable<RefundListViewModel>> GetRefundsForUserAsync(Guid userId)
            => GetByUserAsync(userId);

        public async Task<RefundDetailViewModel?> GetByIdAsync(Guid id)
        {
            var refund = await repo.AllReadonly<DataRefund>()
                .FirstOrDefaultAsync(r => r.Id == id);

            return refund == null ? null : MapDetail(refund);
        }

        public async Task<bool> ApproveAsync(Guid id, Guid processedBy)
        {
            var entity = await repo.All<DataRefund>()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null || entity.Status != RefundStatus.Pending)
            {
                return false;
            }

            entity.Status = RefundStatus.Approved;
            entity.ProcessedBy = processedBy;
            entity.ProcessedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(Guid id, Guid processedBy)
        {
            var entity = await repo.All<DataRefund>()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null || entity.Status != RefundStatus.Pending)
            {
                return false;
            }

            entity.Status = RefundStatus.Rejected;
            entity.ProcessedBy = processedBy;
            entity.ProcessedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteAsync(Guid id)
        {
            var entity = await repo.All<DataRefund>()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null || entity.Status != RefundStatus.Approved)
            {
                return false;
            }

            entity.Status = RefundStatus.Completed;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<RefundOperationResult> RequestTicketRefundAsync(Guid ticketId, Guid requestedBy, string? reason)
        {
            var ticket = await repo.AllReadonly<Ticket>()
                .FirstOrDefaultAsync(t => t.Id == ticketId && t.UserId == requestedBy);

            if (ticket == null)
            {
                return RefundOperationResult.Failed("Messages.Refund.TicketNotFound");
            }

            if (ticket.Status != TicketStatus.Purchased)
            {
                return RefundOperationResult.Failed("Messages.Refund.NotPurchased");
            }

            var ev = await repo.AllReadonly<Event>()
                .FirstOrDefaultAsync(e => e.Id == ticket.EventId);

            if (ev == null || !ev.IsActive)
            {
                return RefundOperationResult.Failed("Messages.Refund.EventNotEligible");
            }

            if (!ev.AllowRefunds)
            {
                return RefundOperationResult.Failed("Messages.Refund.Disabled");
            }

            if (DateTime.UtcNow > ev.StartDateTime.AddHours(-48))
            {
                return RefundOperationResult.Failed("Messages.Refund.TooLate");
            }

            var existingRefund = await repo.AllReadonly<DataRefund>()
                .AnyAsync(r => r.TicketId == ticketId);

            if (existingRefund)
            {
                return RefundOperationResult.Failed("Messages.Refund.Duplicate");
            }

            var payment = await repo.AllReadonly<PaymentTicket>()
                .Where(pt => pt.TicketId == ticketId)
                .Join(
                    repo.AllReadonly<Payment>(),
                    pt => pt.PaymentId,
                    payment => payment.Id,
                    (_, payment) => payment)
                .FirstOrDefaultAsync(payment => payment.Status == Payment.PaymentStatus.Accepted);

            if (payment == null)
            {
                return RefundOperationResult.Failed("Messages.Refund.PaymentNotFound");
            }

            if (string.IsNullOrWhiteSpace(payment.StripePaymentIntentId))
            {
                return RefundOperationResult.Failed("Messages.Refund.PaymentIntentMissing");
            }

            var refundAmount = CalculateRefundAmount(ticket.Price);
            var refund = new DataRefund
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                TicketId = ticketId,
                RequestedBy = requestedBy,
                ProcessedBy = Guid.Empty,
                Amount = refundAmount,
                Currency = ticket.Currency ?? payment.Currency ?? "EUR",
                Reason = NormalizeText(reason),
                Status = RefundStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(refund);
            await repo.SaveChangesAsync();

            return RefundOperationResult.Succeeded(refund.Id, refund.Amount);
        }

        public async Task<IEnumerable<OrganizerRefundListItemViewModel>> GetRefundsForOrganizerAsync(
            Guid organizerId,
            RefundStatus? statusFilter = null)
        {
            var refundRows = await repo.AllReadonly<DataRefund>()
                .Where(r => r.TicketId != null)
                .Join(
                    repo.AllReadonly<Ticket>(),
                    refund => refund.TicketId,
                    ticket => ticket.Id,
                    (refund, ticket) => new { refund, ticket })
                .Join(
                    repo.AllReadonly<Event>(),
                    row => row.ticket.EventId,
                    ev => ev.Id,
                    (row, ev) => new { row.refund, row.ticket, ev })
                .Where(row => row.ev.OrganizerId == organizerId)
                .ToListAsync();

            if (statusFilter.HasValue)
            {
                refundRows = refundRows
                    .Where(row => row.refund.Status == statusFilter.Value)
                    .ToList();
            }

            var userIds = refundRows
                .Select(row => row.ticket.UserId.ToString())
                .Distinct()
                .ToList();

            var users = await repo.AllReadonly<User>()
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
            var userById = users.ToDictionary(u => u.Id);

            return refundRows
                .OrderByDescending(row => row.refund.CreatedAt)
                .Select(row =>
                {
                    userById.TryGetValue(row.ticket.UserId.ToString(), out var buyer);

                    return new OrganizerRefundListItemViewModel
                    {
                        RefundId = row.refund.Id,
                        TicketId = row.ticket.Id,
                        TicketNumber = row.ticket.TicketNumber,
                        EventId = row.ev.Id,
                        EventName = row.ev.EventName ?? "Unknown Event",
                        EventStart = row.ev.StartDateTime,
                        BuyerId = row.ticket.UserId,
                        BuyerDisplay = BuildBuyerDisplay(buyer, row.ticket.UserId),
                        BuyerEmail = buyer?.Email,
                        OriginalAmount = row.ticket.Price,
                        RefundAmount = row.refund.Amount,
                        Currency = row.refund.Currency ?? row.ticket.Currency ?? "EUR",
                        Status = row.refund.Status,
                        Reason = row.refund.Reason,
                        ProcessorComment = row.refund.ProcessorComment,
                        RequestedAt = row.refund.CreatedAt,
                        ProcessedAt = row.refund.ProcessedAt == default ? null : row.refund.ProcessedAt
                    };
                })
                .ToList();
        }

        public async Task<RefundOperationResult> ApproveTicketRefundAsync(Guid refundId, Guid organizerId)
        {
            if (paymentRefundProcessor == null)
            {
                return RefundOperationResult.Failed("Messages.Refund.ProcessorUnavailable");
            }

            var refund = await repo.All<DataRefund>()
                .FirstOrDefaultAsync(r => r.Id == refundId);

            if (refund == null || refund.Status != RefundStatus.Pending || refund.TicketId == null)
            {
                return RefundOperationResult.Failed("Messages.Refund.NotFound");
            }

            var ticket = await repo.All<Ticket>()
                .FirstOrDefaultAsync(t => t.Id == refund.TicketId.Value);
            if (ticket == null || ticket.Status != TicketStatus.Purchased)
            {
                return RefundOperationResult.Failed("Messages.Refund.ApprovalIneligible");
            }

            var ev = await repo.All<Event>()
                .FirstOrDefaultAsync(e => e.Id == ticket.EventId);
            if (ev == null || ev.OrganizerId != organizerId)
            {
                return RefundOperationResult.Failed("Messages.Refund.UnauthorizedOrganizer");
            }

            var payment = await repo.All<Payment>()
                .FirstOrDefaultAsync(p => p.Id == refund.PaymentId);
            if (payment == null || payment.Status != Payment.PaymentStatus.Accepted)
            {
                return RefundOperationResult.Failed("Messages.Refund.PaymentNotFound");
            }

            if (string.IsNullOrWhiteSpace(payment.StripePaymentIntentId))
            {
                return RefundOperationResult.Failed("Messages.Refund.PaymentIntentMissing");
            }

            var stripeResult = await paymentRefundProcessor.RefundPaymentIntentAsync(new PaymentRefundRequest
            {
                PaymentIntentId = payment.StripePaymentIntentId,
                AmountMinor = CalculateMinorAmount(refund.Amount),
                Metadata = new Dictionary<string, string>
                {
                    ["refundId"] = refund.Id.ToString(),
                    ["ticketId"] = ticket.Id.ToString(),
                    ["paymentId"] = payment.Id.ToString(),
                    ["eventId"] = ev.Id.ToString(),
                    ["requesterId"] = refund.RequestedBy.ToString(),
                    ["organizerId"] = organizerId.ToString()
                }
            });

            if (!stripeResult.Success || string.IsNullOrWhiteSpace(stripeResult.StripeRefundId))
            {
                return RefundOperationResult.Failed(stripeResult.ErrorMessage ?? "Messages.Refund.StripeFailed");
            }

            var nowUtc = DateTime.UtcNow;

            refund.Status = RefundStatus.Completed;
            refund.StripeRefundId = stripeResult.StripeRefundId;
            refund.ProcessedBy = organizerId;
            refund.ProcessedAt = nowUtc;
            refund.UpdatedAt = nowUtc;

            ticket.Status = TicketStatus.Refunded;

            if (ev.TicketsSold > 0)
            {
                ev.TicketsSold -= 1;
            }

            ev.UpdatedAt = nowUtc;

            repo.Update(refund);
            repo.Update(ticket);
            repo.Update(ev);

            if (ticket.PricingTierId != Guid.Empty)
            {
                var tier = await repo.All<EventPricingTier>()
                    .FirstOrDefaultAsync(t => t.Id == ticket.PricingTierId);

                if (tier != null && tier.SoldQuantity > 0)
                {
                    tier.SoldQuantity -= 1;
                    tier.UpdatedAt = nowUtc;
                    repo.Update(tier);
                }
            }

            if (await AreAllPaymentTicketsRefundedAsync(payment.Id, ticket.Id))
            {
                payment.Status = Payment.PaymentStatus.Refunded;
                payment.RefundedAt = nowUtc;
                payment.UpdatedAt = nowUtc;
                repo.Update(payment);
            }

            await repo.SaveChangesAsync();

            return RefundOperationResult.Succeeded(refund.Id, refund.Amount);
        }

        public async Task<RefundOperationResult> RejectTicketRefundAsync(Guid refundId, Guid organizerId, string? comment)
        {
            var refund = await repo.All<DataRefund>()
                .FirstOrDefaultAsync(r => r.Id == refundId);

            if (refund == null || refund.Status != RefundStatus.Pending || refund.TicketId == null)
            {
                return RefundOperationResult.Failed("Messages.Refund.NotFound");
            }

            var ticket = await repo.AllReadonly<Ticket>()
                .FirstOrDefaultAsync(t => t.Id == refund.TicketId.Value);
            if (ticket == null)
            {
                return RefundOperationResult.Failed("Messages.Refund.TicketNotFound");
            }

            var ev = await repo.AllReadonly<Event>()
                .FirstOrDefaultAsync(e => e.Id == ticket.EventId);
            if (ev == null || ev.OrganizerId != organizerId)
            {
                return RefundOperationResult.Failed("Messages.Refund.UnauthorizedOrganizer");
            }

            var nowUtc = DateTime.UtcNow;

            refund.Status = RefundStatus.Rejected;
            refund.ProcessedBy = organizerId;
            refund.ProcessedAt = nowUtc;
            refund.ProcessorComment = NormalizeText(comment);
            refund.UpdatedAt = nowUtc;

            repo.Update(refund);
            await repo.SaveChangesAsync();

            return RefundOperationResult.Succeeded(refund.Id, refund.Amount);
        }

        private async Task<bool> AreAllPaymentTicketsRefundedAsync(Guid paymentId, Guid currentRefundedTicketId)
        {
            var paymentTicketIds = await repo.AllReadonly<PaymentTicket>()
                .Where(pt => pt.PaymentId == paymentId)
                .Select(pt => pt.TicketId)
                .ToListAsync();

            if (paymentTicketIds.Count == 0)
            {
                return true;
            }

            var otherTicketStatuses = await repo.AllReadonly<Ticket>()
                .Where(t => paymentTicketIds.Contains(t.Id) && t.Id != currentRefundedTicketId)
                .Select(t => t.Status)
                .ToListAsync();

            return otherTicketStatuses.All(status => status == TicketStatus.Refunded);
        }

        private static RefundListViewModel MapListItem(DataRefund refund)
            => new()
            {
                Id = refund.Id,
                PaymentId = refund.PaymentId,
                TicketId = refund.TicketId,
                RequestedBy = refund.RequestedBy,
                Amount = refund.Amount,
                Currency = refund.Currency,
                Reason = refund.Reason,
                ProcessorComment = refund.ProcessorComment,
                Status = refund.Status,
                CreatedAt = refund.CreatedAt,
                ProcessedAt = refund.ProcessedAt == default ? null : refund.ProcessedAt
            };

        private static RefundDetailViewModel MapDetail(DataRefund refund)
            => new()
            {
                Id = refund.Id,
                PaymentId = refund.PaymentId,
                TicketId = refund.TicketId,
                RequestedBy = refund.RequestedBy,
                ProcessedBy = refund.ProcessedBy,
                Amount = refund.Amount,
                Currency = refund.Currency,
                Reason = refund.Reason,
                ProcessorComment = refund.ProcessorComment,
                Status = refund.Status,
                CreatedAt = refund.CreatedAt,
                UpdatedAt = refund.UpdatedAt,
                ProcessedAt = refund.ProcessedAt == default ? null : refund.ProcessedAt
            };

        private static float CalculateRefundAmount(float ticketPrice)
            => (float)Math.Round((decimal)ticketPrice * (decimal)RefundRate, 2, MidpointRounding.AwayFromZero);

        private static long CalculateMinorAmount(float refundAmount)
            => (long)Math.Round((decimal)refundAmount * 100m, 0, MidpointRounding.AwayFromZero);

        private static string? NormalizeText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return text.Trim();
        }

        private static string BuildBuyerDisplay(User? user, Guid buyerId)
        {
            if (user == null)
            {
                return buyerId.ToString();
            }

            if (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
            {
                return $"{user.FirstName} {user.LastName}".Trim();
            }

            return user.UserName ?? user.Email ?? buyerId.ToString();
        }
    }
}
