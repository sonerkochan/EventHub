using System;
using System.ComponentModel.DataAnnotations;

namespace EventHub.Core.Models.EventPricingTier
{
    public class CreatePricingTierViewModel
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        public Guid ZoneId { get; set; }

        [Required]
        [StringLength(100)]
        public string TierName { get; set; } = null!;

        [Required]
        [Range(0f, float.MaxValue)]
        public float Price { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [Required]
        [Range(1, int.MaxValue)]
        public int AvailableQuantity { get; set; }
    }
}
