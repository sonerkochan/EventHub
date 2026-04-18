using System;

namespace EventHub.Core.Models.Payment
{
    public class CreateCheckoutRequest
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; } = "eur";
        public string EventName { get; set; } = null!;
        public string SuccessUrl { get; set; } = null!;
        public string CancelUrl { get; set; } = null!;
    }
}