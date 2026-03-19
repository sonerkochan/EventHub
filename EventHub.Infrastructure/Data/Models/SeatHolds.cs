using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class SeatHolds
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid SeatId { get; set; }
        public Guid UserId { get; set; }
        public Guid SessionId { get; set; }
        public DateTime HeldAt { get; set; }
        public DateTime ReleasedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
