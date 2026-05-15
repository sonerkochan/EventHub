using System;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Admin
{
    public class ManagedZoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ZoneType ZoneType { get; set; }
        public int SeatCount { get; set; }
        public int SoldCount { get; set; }
        public float? Price { get; set; }
        public string? Currency { get; set; }
        public bool HasPricing => Price.HasValue;
    }
}
