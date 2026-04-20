using System;
using System.ComponentModel.DataAnnotations;
using static EventHub.Infrastructure.Data.Models.Review;

namespace EventHub.Core.Models.Review
{
    public class CreateReviewViewModel
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        public ReviewRating Rating { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Content { get; set; }
    }
}
