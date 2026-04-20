using System;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Zone
{
    public class ZoneListViewModel
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public string? RoomName { get; set; }
        public string? Name { get; set; }
        public ZoneType ZoneType { get; set; }
        public int Capacity { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
