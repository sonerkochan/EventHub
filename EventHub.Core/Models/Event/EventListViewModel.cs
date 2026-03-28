using EventHub.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Core.Models.Event
{
    public class EventListViewModel
    {
        public Guid Id { get; set; }
        public string EventName { get; set; } = null!;
        public EventType EventType { get; set; }
        public EventStatus EventStatus { get; set; }
        public EventPriority EventPriority { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int TotalTickets { get; set; }
        public int TicketsSold { get; set; }
        public bool IsActive { get; set; }
        public string? RoomName { get; set; }
    }
}
