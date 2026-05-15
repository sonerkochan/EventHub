using System;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Admin
{
    public class AdminTicketLookupDto
    {
        public Guid Id { get; set; }
        public long TicketNumber { get; set; }
        public string? HashedCode { get; set; }

        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public DateTime EventStart { get; set; }
        public string? RoomName { get; set; }

        public int SeatNumber { get; set; }
        public string? ZoneName { get; set; }

        public string BuyerDisplay { get; set; } = "";
        public string? BuyerEmail { get; set; }

        public TicketStatus Status { get; set; }
        public float Price { get; set; }
        public string? Currency { get; set; }

        public DateTime ReservedAt { get; set; }
        public DateTime PurchasedAt { get; set; }
        public DateTime ValidatedAt { get; set; }

        public bool CanRefund => Status == TicketStatus.Purchased || Status == TicketStatus.Reserved;
    }
}
