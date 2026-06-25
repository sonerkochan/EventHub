using System;
using System.Collections.Generic;
using System.Text;
using EventHub.Infrastructure.Data.Models;
using RefundStatus = EventHub.Infrastructure.Data.Models.Refund.RefundStatus;

namespace EventHub.Core.Models.Ticket
{
    public class TicketDetailViewModel
    {
        public Guid Id { get; set; }
        public long TicketNumber { get; set; }
        public string HashedCode { get; set; } = null!;
        public string? QRCodeImage { get; set; }
        public string EventName { get; set; } = null!;
        public string? EventDescription { get; set; }
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        public string RoomName { get; set; } = null!;
        public float Price { get; set; }
        public string Currency { get; set; } = "EUR";
        public bool IsUsed { get; set; }
        public DateTime PurchasedAt { get; set; }
        public TicketStatus Status { get; set; }
        public bool CanRequestRefund { get; set; }
        public RefundStatus? RefundStatus { get; set; }
        public float RefundAmount { get; set; }
        public string? RefundReason { get; set; }
        public string? RefundProcessorComment { get; set; }
        public DateTime? RefundRequestedAt { get; set; }
    }
}
