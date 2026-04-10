using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Room
{
    public class SaveSeatLayoutRequest
    {
        public Guid RoomId { get; set; }
        public string? LayoutName { get; set; }
        public int GridRows { get; set; }
        public int GridColumns { get; set; }
        public List<SeatDto> Seats { get; set; } = new();
    }

    public class CreateZoneRequest
    {
        public Guid RoomId { get; set; }
        public string Name { get; set; } = null!;
        public ZoneType ZoneType { get; set; }
    }

    public class AssignZoneRequest
    {
        public Guid RoomId { get; set; }
        public Guid ZoneId { get; set; }
        public List<Guid> SeatIds { get; set; } = new();
    }

    public class RemoveFromZoneRequest
    {
        public Guid RoomId { get; set; }
        public List<Guid> SeatIds { get; set; } = new();
    }

    public class DeleteZoneRequest
    {
        public Guid Id { get; set; }
    }
}
