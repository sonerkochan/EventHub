using System;
using static EventHub.Infrastructure.Data.Models.Refund;

namespace EventHub.Core.Models.Refund
{
    public class RefundListViewModel
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public Guid? TicketId { get; set; }
        public Guid RequestedBy { get; set; }
        public float Amount { get; set; }
        public string? Currency { get; set; }
        public string? Reason { get; set; }
        public string? ProcessorComment { get; set; }
        public RefundStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
