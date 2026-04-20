using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class EmailLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TemplateId { get; set; }
        public string? RecipientEmail { get; set; }
        public string? SenderEmail { get; set; }
        public string? Subject { get; set; }
        public EmailStatus Status { get; set; }
        public DateTime SentAt { get; set; }
        public string? FailureReason { get; set; }
        public Guid? ExternalMessageId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public enum EmailStatus
        {
            Pending,
            Sent,
            Failed,
            Bounced
        }
    }
}
