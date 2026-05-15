using System;
using System.ComponentModel.DataAnnotations;

namespace EventHub.Core.Models.Admin
{
    public class SetZonePriceRequest
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        public Guid ZoneId { get; set; }

        [Required]
        [Range(0f, float.MaxValue)]
        public float Price { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "EUR";
    }
}
