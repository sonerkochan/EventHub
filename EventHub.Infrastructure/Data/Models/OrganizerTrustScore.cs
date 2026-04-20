using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class OrganizerTrustScore
    {
        public Guid Id { get; set; }
        public Guid OrganizerId { get; set; }
        [Range(0,100)]
        public int ScoreIndex { get; set; }
        public int TotalEvents { get; set; }
        public int CompletedEvents { get; set; }
        public int CancelledEvents { get; set; }
        public float AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalRefunds { get; set; }
        public float RefundRate { get; set; }
        public DateTime LastCalculatedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
