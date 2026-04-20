using System;

namespace EventHub.Core.Models.Seat
{
    public class SeatListViewModel
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Guid? ZoneId { get; set; }
        public string? ZoneName { get; set; }
        public int SeatNumber { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsActive { get; set; }
    }
}
