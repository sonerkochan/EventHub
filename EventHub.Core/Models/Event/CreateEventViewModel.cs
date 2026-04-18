using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHub.Core.Models.Event
{
    public class CreateEventViewModel
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string EventName { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public EventType EventType { get; set; }

        [Required]
        public EventPriority EventPriority { get; set; }

        [Required]
        public Guid RoomId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartDateTime { get; set; } = DateTime.Today.AddDays(1);

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndDateTime { get; set; } = DateTime.Today.AddDays(1).AddHours(2);

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Must have at least 1 ticket.")]
        public int TotalTickets { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be 0 or greater.")]
        public decimal BasePrice { get; set; } = 0;

        public bool AllowRefunds { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? RefundDeadline { get; set; }

        public string? CoverImageUrl { get; set; }

        // populated from controller!
        public IEnumerable<SelectListItem> AvailableRooms { get; set; } = [];
    }
}
