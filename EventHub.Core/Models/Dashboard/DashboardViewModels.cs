using EventHub.Core.Contracts;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventHub.Core.Models.Dashboard
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalEvents { get; set; }
        public int PublishedEvents { get; set; }
        public int TotalTicketsSold { get; set; }
        public int PendingApplications { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveVenues { get; set; }
        public int TodayRegistrations { get; set; }
    }

    public class OrganizerDashboardViewModel
    {
        public int MyEvents { get; set; }
        public int PublishedEvents { get; set; }
        public int DraftEvents { get; set; }
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int UpcomingEventsCount { get; set; }
    }
}
