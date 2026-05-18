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
        public string? Description { get; set; }
        public EventType EventType { get; set; }
        public EventStatus EventStatus { get; set; }
        public EventPriority EventPriority { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? CountryCode { get; set; }
        public int TotalTickets { get; set; }
        public int TicketsSold { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? PriceAmount { get; set; }
        public decimal? DisplayPrice { get; set; }
        public string DisplayCurrency { get; set; } = "EUR";
        public string PriceText { get; set; } = "Free";
        public bool IsFree { get; set; } = true;
        public bool IsActive { get; set; }
        public string? RoomName { get; set; }
        public string? CoverImageUrl { get; set; }
    }
}
