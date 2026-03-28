using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHub.Core.Models.Event
{
    public class EditEventViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string EventName { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public EventType EventType { get; set; }

        [Required]
        public EventStatus EventStatus { get; set; }

        [Required]
        public EventPriority EventPriority { get; set; }

        [Required]
        public Guid RoomId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartDateTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndDateTime { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int TotalTickets { get; set; }

        public bool AllowRefunds { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? RefundDeadline { get; set; }

        public string? CoverImageUrl { get; set; }

        public IEnumerable<SelectListItem> AvailableRooms { get; set; } = [];
    }
}
