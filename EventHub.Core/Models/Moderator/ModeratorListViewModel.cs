using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Core.Models.Moderator
{
    public class ModeratorListViewModel
    {
        public string Id { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
