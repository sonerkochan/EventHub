using System;
using System.Collections.Generic;

namespace EventHub.Core.Models.Admin
{
    public class EventTicketsManageViewModel
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public Guid RoomId { get; set; }
        public string? RoomName { get; set; }
        public DateTime EventStart { get; set; }
        public int GridRows { get; set; } = 10;
        public int GridColumns { get; set; } = 10;
        public List<ManagedZoneDto> Zones { get; set; } = new();
        public List<ManagedSeatDto> Seats { get; set; } = new();
        public List<AdminTicketRow> Tickets { get; set; } = new();

        public int TotalSeats => Seats.Count;
        public int SoldSeats { get; set; }
        public int ReservedSeats { get; set; }
        public int FreeSeats => Math.Max(0, TotalSeats - SoldSeats - ReservedSeats);
        public float TotalRevenue { get; set; }
        public string Currency { get; set; } = "EUR";
        public float BasePrice { get; set; }
        public int UnzonedSeatCount { get; set; }
        public int UnzonedSoldCount { get; set; }
        public bool HasZonesWithoutTier { get; set; }
    }
}
