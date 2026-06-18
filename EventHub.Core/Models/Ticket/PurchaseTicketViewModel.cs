using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventHub.Core.Models.Ticket
{
    public class PurchaseTicketViewModel
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public DateTime EventStart { get; set; }
        public string RoomName { get; set; } = null!;
        public float Price { get; set; }
        public int AvailableTickets { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Validation.Ticket.Quantity.Range")]
        public int Quantity { get; set; } = 1;

        public Guid RoomId { get; set; }
        public int GridRows { get; set; }
        public int GridColumns { get; set; }
        public List<ClientSeatDto> Seats { get; set; } = new();
        public List<ClientZoneDto> Zones { get; set; } = new();
        public float BasePrice { get; set; }
        public string Currency { get; set; } = "EUR";

        public bool HasSeatLayout => Seats.Count > 0;
    }
}
