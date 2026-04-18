using System;

namespace EventHub.Core.Models.Ticket
{
    public class TicketDetailViewModel
    {
        public Guid Id { get; set; }
        public long TicketNumber { get; set; }
        public string HashedCode { get; set; } = null!;
        public string EventName { get; set; } = null!;
        public string? EventDescription { get; set; }
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        public string RoomName { get; set; } = null!;
        public float Price { get; set; }
        public string Currency { get; set; } = "USD";
        public bool IsUsed { get; set; }
        public DateTime PurchasedAt { get; set; }
    }
}