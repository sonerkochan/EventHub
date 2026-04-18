using System;
using System.Collections.Generic;

namespace EventHub.Core.Models.User
{
    public class UserDetailViewModel
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}