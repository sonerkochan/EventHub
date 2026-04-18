using System;

namespace EventHub.Core.Models.Ticket
{
    public class TicketListViewModel
    {
        public Guid Id { get; set; }
        public long TicketNumber { get; set; }
        public string EventName { get; set; } = null!;
        public DateTime EventStart { get; set; }
        public string RoomName { get; set; } = null!;
        public float Price { get; set; }
        public string Currency { get; set; } = "USD";
        public bool IsUsed { get; set; }
        public DateTime PurchasedAt { get; set; }
    }
}