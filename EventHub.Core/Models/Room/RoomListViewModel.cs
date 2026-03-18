using EventHub.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Core.Models.Room
{
    public class RoomListViewModel
    {
        public Guid Id { get; set; }
        public Guid VenueId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public long Capacity { get; set; }
        public RoomType RoomType { get; set; }
        public bool IsActive { get; set; }
    }
}
