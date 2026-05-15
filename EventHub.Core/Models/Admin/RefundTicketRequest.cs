using System;
using System.ComponentModel.DataAnnotations;

namespace EventHub.Core.Models.Admin
{
    public class RefundTicketRequest
    {
        [Required]
        public Guid TicketId { get; set; }
    }
}
