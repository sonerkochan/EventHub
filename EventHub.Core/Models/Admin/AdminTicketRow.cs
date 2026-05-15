using System;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Admin
{
    public class AdminTicketRow
    {
        public Guid Id { get; set; }
        public long TicketNumber { get; set; }
        public Guid SeatId { get; set; }
        public int SeatNumber { get; set; }
        public Guid? ZoneId { get; set; }
        public string? ZoneName { get; set; }
        public TicketStatus Status { get; set; }
        public float Price { get; set; }
        public string? Currency { get; set; }
        public Guid BuyerUserId { get; set; }
        public string BuyerDisplay { get; set; } = "";
        public DateTime ReservedAt { get; set; }
        public DateTime PurchasedAt { get; set; }
    }
}
