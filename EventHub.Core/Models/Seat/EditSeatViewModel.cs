using System;
using System.ComponentModel.DataAnnotations;

namespace EventHub.Core.Models.Seat
{
    public class EditSeatViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid RoomId { get; set; }

        public Guid? ZoneId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int SeatNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Row { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Column { get; set; }

        public float PositionX { get; set; }
        public float PositionY { get; set; }
    }
}
