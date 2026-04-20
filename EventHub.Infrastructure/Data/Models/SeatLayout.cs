using System;
using System.Collections.Generic;
using System.Text;
namespace EventHub.Infrastructure.Data.Models
{
    public class SeatLayout
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Guid CreatedBy { get; set; }
        public string? Name { get; set; }
        public string? Structure { get; set; }
        public string? Description { get; set; }
        public int TotalSeats { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
