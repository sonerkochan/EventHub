using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class ApplicationForm
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public string OrganizationName { get; set; } = null!; // Business name

        public string PhoneNumber { get; set; } = null!;

        public ApplicationType Type { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public string? Description { get; set; }

        public string? ReviewedById { get; set; }
        public User? ReviewedBy { get; set; }

        public string? ReviewComment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ReviewedAt { get; set; }
    }
    public enum ApplicationType
    {
        Organizer = 1,
        Supplier = 2
    }

    public enum ApplicationStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}
