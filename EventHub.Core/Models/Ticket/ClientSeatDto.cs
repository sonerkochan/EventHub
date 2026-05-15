using System;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Ticket
{
    public class ClientSeatDto
    {
        public Guid Id { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int SeatNumber { get; set; }
        public Guid? ZoneId { get; set; }
        public string? ZoneName { get; set; }
        public ZoneType? ZoneType { get; set; }
        public float Price { get; set; }
        public string Currency { get; set; } = "EUR";
        public bool UsesBasePrice { get; set; }
        public bool IsTaken { get; set; }
    }
}
