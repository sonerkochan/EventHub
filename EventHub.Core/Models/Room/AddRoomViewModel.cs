using EventHub.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHub.Core.Models.Room
{
    public class AddRoomViewModel
    {

        [Required]
        public Guid VenueId { get; set; }

        [Required]
        public Guid CreatedBy { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public long Capacity { get; set; }

        [Required]
        public RoomType RoomType { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
