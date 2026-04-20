using System;
using System.ComponentModel.DataAnnotations;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Zone
{
    public class CreateZoneViewModel
    {
        [Required]
        public Guid RoomId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        public ZoneType ZoneType { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        public int DisplayOrder { get; set; }
    }
}
