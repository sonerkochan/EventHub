using System;
using RefundStatus = EventHub.Infrastructure.Data.Models.Refund.RefundStatus;

namespace EventHub.Core.Models.Refund
{
    public class OrganizerRefundListItemViewModel
    {
        public Guid RefundId { get; set; }
        public Guid TicketId { get; set; }
        public long TicketNumber { get; set; }
        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public DateTime EventStart { get; set; }
        public Guid BuyerId { get; set; }
        public string BuyerDisplay { get; set; } = null!;
        public string? BuyerEmail { get; set; }
        public float OriginalAmount { get; set; }
        public float RefundAmount { get; set; }
        public string Currency { get; set; } = "EUR";
        public RefundStatus Status { get; set; }
        public string? Reason { get; set; }
        public string? ProcessorComment { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
