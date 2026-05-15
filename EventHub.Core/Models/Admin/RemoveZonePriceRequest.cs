using System;
using System.ComponentModel.DataAnnotations;

namespace EventHub.Core.Models.Admin
{
    public class RemoveZonePriceRequest
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        public Guid ZoneId { get; set; }
    }
}
