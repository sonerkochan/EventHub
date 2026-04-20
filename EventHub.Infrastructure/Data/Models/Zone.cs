using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace EventHub.Infrastructure.Data.Models
{
    public class Zone
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Guid CreatedBy { get; set; }
        public string? Name { get; set; }
        public ZoneType ZoneType { get; set; }
        public int Capacity { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum ZoneType
    {
        VIP,
        Regular,
        Economy
    }
}
