using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class EventPricingTier
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid ZoneId { get; set; }
        public string? TierName { get; set; }
        public float Price { get; set; }
        public string? Currency { get; set; }
        public int AvailableQuantity { get; set; }
        public int SoldQuantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }
}
