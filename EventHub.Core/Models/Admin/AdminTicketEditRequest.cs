using System;
using System.Text.Json.Serialization;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Admin
{
    public class AdminTicketEditRequest
    {
        public Guid TicketId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TicketStatus Status { get; set; }

        public Guid SeatId { get; set; }
    }
}
