using System;
using System.Collections.Generic;

namespace EventHub.Core.Models.Ticket
{
    public class ReserveSeatsResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<Guid> TicketIds { get; set; } = new();
        public float TotalPrice { get; set; }
        public string Currency { get; set; } = "EUR";
        public List<ReservedSeatLine> Lines { get; set; } = new();
    }

    public class ReservedSeatLine
    {
        public Guid TicketId { get; set; }
        public Guid SeatId { get; set; }
        public int SeatNumber { get; set; }
        public string? ZoneName { get; set; }
        public float Price { get; set; }
        public string Currency { get; set; } = "EUR";
    }
}
