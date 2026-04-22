using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Core.Models.Ticket
{
    public class TicketValidationResult
    {
        public Guid TicketId { get; set; }
        public long TicketNumber { get; set; }
        public string EventName { get; set; } = null!;
        public DateTime EventStart { get; set; }
        public string RoomName { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public float Price { get; set; }
        public string Currency { get; set; } = null!;
        public bool WasAlreadyUsed { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}
