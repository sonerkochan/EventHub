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
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public int TotalTickets { get; set; }
        public int TicketsSold { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; }
        public string? RoomName { get; set; }
        public string? CoverImageUrl { get; set; }
    }
}
