using System;
using System.Collections.Generic;

namespace EventHub.Core.Models.Payment
{
    public class CreateSeatCheckoutRequest
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public string Currency { get; set; } = "eur";
        public string EventName { get; set; } = null!;
        public string SuccessUrl { get; set; } = null!;
        public string CancelUrl { get; set; } = null!;
        public List<CheckoutSeatLine> Lines { get; set; } = new();
    }

    public class CheckoutSeatLine
    {
        public Guid TicketId { get; set; }
        public int SeatNumber { get; set; }
        public string? ZoneName { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
