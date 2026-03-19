using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TicketId { get; set; }
        public Guid StripePaymentIntentId { get; set; }
        public Guid StripeSessionId { get; set; }
        public float Amount { get; set; }
        public string? Currency { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentMethod Method { get; set; }
        public string? FailureReason { get; set; }
        public List<string?> Metadata { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime SucceededAt { get; set; }
        public DateTime FailedAt { get; set; }
        public DateTime RefundedAt { get; set; }

        public enum PaymentMethod
        {
            Card,
            PayPal,
            ApplePay,
            GooglePay,
            BankTransfer,
            Other
        }
        public enum PaymentStatus
        {
            Accepted,
            Declined,
            OnHold,
            Refunded
        }
    }
}
