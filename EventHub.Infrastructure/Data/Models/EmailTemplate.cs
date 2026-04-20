using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class EmailTemplate
    {
        public Guid Id { get; set; }
        public string? TemplateName { get; set; }
        public string? Subject { get; set; }
        public string? BodyHtml { get; set; }
        public string? BodyText { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
