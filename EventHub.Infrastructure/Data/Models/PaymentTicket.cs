using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class PaymentTicket
    {
        public Guid PaymentId { get; set; }
        public Guid TicketId { get; set; }
    }
}
