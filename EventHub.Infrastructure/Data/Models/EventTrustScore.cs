using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class EventTrustScore
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }

        [Range(0, 100)]
        public int ScoreIndex { get; set; }
        public float AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalRefunds { get; set; }
        public float RefundRate { get; set; }
        public DateTime LastCalculatedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
