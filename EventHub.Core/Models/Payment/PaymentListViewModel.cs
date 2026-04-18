using System;

namespace EventHub.Core.Models.Payment
{
    public class PaymentListViewModel
    {
        public Guid Id { get; set; }
        public string EventName { get; set; } = null!;
        public float Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}