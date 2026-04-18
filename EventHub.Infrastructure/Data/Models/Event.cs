using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class Event
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Guid OrganizerId { get; set; }
        public string? EventName { get; set; }
        public string? Description { get; set; }
        public EventType EventType { get; set; }
        public EventStatus EventStatus { get; set; }
        public EventPriority EventPriority { get; set; }
        public string? Type { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int TotalTickets { get; set; }
        public int TicketsSold { get; set; }
        public decimal BasePrice { get; set; } = 0;
        public bool AllowRefunds { get; set; }
        public DateTime RefundDeadline { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string? CoverImageUrl { get; set; }
    }

    public enum EventPriority
    {
        Paid,
        GoodReputation,
        Normal
    }

    public enum EventStatus
    {
        Active,
        Draft,
        Published,
        Cancelled,
        Finished,
    }
    public enum EventType
    {
        Conference,
        Workshop,
        Seminar,
        Concert,
        Sports,
        Other
    }
}
