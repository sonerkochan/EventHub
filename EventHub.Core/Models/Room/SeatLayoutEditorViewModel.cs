using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Room
{
    public class SeatLayoutEditorViewModel
    {
        public Guid RoomId { get; set; }
        public string RoomName { get; set; } = null!;
        public long RoomCapacity { get; set; }

        public Guid? LayoutId { get; set; }
        public string? LayoutName { get; set; }
        public int GridRows { get; set; } = 10;
        public int GridColumns { get; set; } = 10;
        public string? StructureJson { get; set; }

        public List<SeatDto> Seats { get; set; } = new();
        public List<ZoneDto> Zones { get; set; } = new();
    }

    public class SeatDto
    {
        public Guid? Id { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int SeatNumber { get; set; }
        public Guid? ZoneId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ZoneDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = null!;
        public ZoneType ZoneType { get; set; }
        public int SeatCount { get; set; }
    }
}
