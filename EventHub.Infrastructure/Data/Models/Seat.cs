using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class Seat
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Guid? ZoneId { get; set; }
        //seeded with default values -> Row 1, Column 1 (First row and col) , positionX 0.0f, postiionY 0.0f (top left corner)
        public int SeatNumber { get; set; }
        public int Row { get; set; } = 1;
        public int Column { get; set; } = 1;
        public float PositionX { get; set; } = 0.0f;
        public float PositionY { get; set; } = 0.0f;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
