using System;
using static EventHub.Infrastructure.Data.Models.Review;

namespace EventHub.Core.Models.Review
{
    public class ReviewListViewModel
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string? EventName { get; set; }
        public Guid UserId { get; set; }
        public ReviewRating Rating { get; set; }
        public string? Title { get; set; }
        public bool IsHidden { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
