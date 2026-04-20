using System;

namespace EventHub.Core.Models.EventPricingTier
{
    public class PricingTierListViewModel
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string? EventName { get; set; }
        public Guid ZoneId { get; set; }
        public string? ZoneName { get; set; }
        public string? TierName { get; set; }
        public float Price { get; set; }
        public string? Currency { get; set; }
        public int AvailableQuantity { get; set; }
        public int SoldQuantity { get; set; }
        public bool IsActive { get; set; }
    }
}
