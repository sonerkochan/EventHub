using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class Refund
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public Guid? TicketId { get; set; }
        public Guid RequestedBy { get; set; }
        public Guid ProcessedBy { get; set; }
        public string? StripeRefundId { get; set; }
        public float Amount { get; set; }
        public string? Currency { get; set; }
        public string? Reason { get; set; }
        public string? ProcessorComment { get; set; }
        public RefundStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ProcessedAt { get; set; }
        public Ticket? Ticket { get; set; }

        public enum RefundStatus
        {
            Pending,
            Approved,
            Rejected,
            Completed
        }
    }
}
