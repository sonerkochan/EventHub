using System;

namespace EventHub.Core.Models.User
{
    public class UserListViewModel
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();

        public int OrganizerEventCount { get; set; }
        public int OrganizerTicketsSold { get; set; }
        public decimal OrganizerRevenue { get; set; }

        public int SupplierServiceCount { get; set; }
        public int SupplierPendingRequests { get; set; }

        public int ClientTicketsBought { get; set; }
    }
}