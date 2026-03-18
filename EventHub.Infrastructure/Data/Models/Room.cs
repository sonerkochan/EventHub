using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class Room
    {
        //[Key]
        public Guid RoomId { get; set; }
        public Guid VenueId { get; set; }
        public Guid CreatedBy { get; set; }
        public string? Name { get; set; } = null;
        public string? Description { get; set; } = null;
        public long Capacity { get; set; }
        public RoomType RoomType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public enum RoomType
    {
        Theatre,
        Auditorium,
        Classroom,
        Banquet,
        Boardroom,
        Arena
    }
}
