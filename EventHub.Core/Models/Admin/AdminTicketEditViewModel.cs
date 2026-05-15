using System;
using System.Collections.Generic;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Admin
{
    public class AdminTicketEditViewModel
    {
        public Guid TicketId { get; set; }
        public long TicketNumber { get; set; }
        public string? HashedCode { get; set; }

        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public string? BuyerDisplay { get; set; }

        public Guid CurrentSeatId { get; set; }
        public int CurrentSeatNumber { get; set; }
        public string? CurrentZoneName { get; set; }

        public TicketStatus CurrentStatus { get; set; }
        public float Price { get; set; }
        public string? Currency { get; set; }

        public List<AdminAvailableSeatOption> AvailableSeats { get; set; } = new();
    }

    public class AdminAvailableSeatOption
    {
        public Guid Id { get; set; }
        public int SeatNumber { get; set; }
        public string? ZoneName { get; set; }
        public bool IsCurrent { get; set; }
    }
}
