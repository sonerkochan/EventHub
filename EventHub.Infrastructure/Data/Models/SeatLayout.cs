using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace EventHub.Infrastructure.Data.Models
{
    public class SeatLayout
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Guid CreatedBy { get; set; }
        public string? Name { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public string? Structure { get; set; }
        public string? Description { get; set; }
        public int TotalSeats { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
