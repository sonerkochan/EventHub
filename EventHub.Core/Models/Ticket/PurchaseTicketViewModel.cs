using System;
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
        [Range(1, 10, ErrorMessage = "You can purchase between 1 and 10 tickets.")]
        public int Quantity { get; set; } = 1;
    }
}