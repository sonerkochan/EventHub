using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class AnalyticsLog
    {
        [Key]
        public int Id { get; set; }

        public string? IpAddress { get; set; }
        public string? Url { get; set; }
        public string? Referrer { get; set; }
        public DateTime Timestamp { get; set; }
        public string Country { get; set; } = string.Empty;
    }
}
