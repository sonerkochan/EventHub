using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class Refund
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public Guid RequestedBy { get; set; }
        public Guid ProcessedBy { get; set; }
        public Guid StripeRefundId { get; set; }
        public float Amount { get; set; }
        public string? Currency { get; set; }
        public string? Reason { get; set; }
        public RefundStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime ProcessedAt { get; set; }

        public enum RefundStatus
        {
            Pending,
            Approved,
            Rejected,
            Completed
        }
    }
}
