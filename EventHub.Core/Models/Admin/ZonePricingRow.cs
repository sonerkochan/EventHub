using System;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Admin
{
    public class ZonePricingRow
    {
        public Guid ZoneId { get; set; }
        public string ZoneName { get; set; } = null!;
        public ZoneType ZoneType { get; set; }
        public int SeatCount { get; set; }

        public Guid? TierId { get; set; }
        public float? Price { get; set; }
        public string? Currency { get; set; }
        public int? AvailableQuantity { get; set; }
        public int SoldQuantity { get; set; }

        public bool IsConfigured => TierId.HasValue;
    }
}
