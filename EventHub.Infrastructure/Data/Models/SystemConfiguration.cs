using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class SystemConfiguration
    {
        public Guid Id { get; set; }
        public Guid CreatedBy { get; set; }
        public int ConfigKey { get; set; }
        public string? ConfigValue { get; set; }
        public string? Description { get; set; }
        public string? DataType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }
}
