using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public Guid HiddenBy { get; set; }
        public ReviewRating Rating { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool IsHidden { get; set; }
        public string? HiddenReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public enum ReviewRating
        {
            OneStar = 1,
            TwoStars = 2,
            ThreeStars = 3,
            FourStars = 4,
            FiveStars = 5
        }
    }
}
