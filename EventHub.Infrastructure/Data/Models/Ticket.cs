using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public Guid SeatId { get; set; }
        public Guid PricingTierId { get; set; }
        public Guid ValidatedBy { get; set; }
        public long TicketNumber { get; set; }
        public TicketStatus Status { get; set; }
        public float Price { get; set; }
        public string? Currency { get; set; }
        public string? HashedCode { get; set; }
        public string? QRCodeImage { get; set; }
        public bool IsUsed { get; set; }
        public DateTime ReservedAt{ get; set; }
        public DateTime ReservationExpiresAt { get; set; }
        public DateTime PurchasedAt { get; set; }
        public DateTime ValidatedAt { get; set; }
    }

    public enum TicketStatus
    {
        Reserved,
        Purchased,
        Used,
        Expired,
        Refunded,
        Cancelled
    }
}
