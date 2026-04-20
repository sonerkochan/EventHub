using System;
using System.ComponentModel.DataAnnotations;

namespace EventHub.Core.Models.Seat
{
    public class CreateSeatViewModel
    {
        [Required]
        public Guid RoomId { get; set; }

        public Guid? ZoneId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int SeatNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Row { get; set; } = 1;

        [Required]
        [Range(1, int.MaxValue)]
        public int Column { get; set; } = 1;

        public float PositionX { get; set; }
        public float PositionY { get; set; }
    }
}
