using System;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Admin
{
    public class TicketsOverviewRow
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public string? RoomName { get; set; }
        public DateTime StartDateTime { get; set; }
        public EventStatus EventStatus { get; set; }
        public int TicketsSold { get; set; }
        public int TotalTickets { get; set; }
        public bool HasPricing { get; set; }
    }
}
