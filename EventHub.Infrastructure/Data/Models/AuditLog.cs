using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class AuditLog
    {
        //int on purpose
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public Guid EntityId { get; set; }
        public string? EntityType { get; set; }
        public string? ActionType { get; set; }
        public string? ActionCategory { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public List<string?> ChangeSet { get; set; } = new();
        public string? Reason { get; set; }
        public List<string?> Metadata { get; set; } = new();
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime OccuredAt { get; set; }
        public DateTime ProcessedAt { get; set; }

    }
}
