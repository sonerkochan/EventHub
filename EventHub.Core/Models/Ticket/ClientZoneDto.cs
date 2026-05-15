using System;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Ticket
{
    public class ClientZoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ZoneType ZoneType { get; set; }
        public int SeatCount { get; set; }
        public int AvailableCount { get; set; }
        public float Price { get; set; }
        public string Currency { get; set; } = "EUR";
        public bool UsesBasePrice { get; set; }
    }
}
