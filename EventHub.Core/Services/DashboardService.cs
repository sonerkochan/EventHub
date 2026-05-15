using EventHub.Core.Contracts;
using EventHub.Core.Models.Dashboard;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventHub.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IRepository repo;

        public DashboardService(IRepository _repo)
        {
            repo = _repo;
        }

        public async Task<AdminDashboardViewModel> GetAdminDashboardAsync()
        {
            var today = DateTime.UtcNow.Date;

            return new AdminDashboardViewModel
            {
                TotalUsers = await repo.AllReadonly<User>()
                    .CountAsync(u => !u.IsDeleted),

                TotalEvents = await repo.AllReadonly<Event>()
                    .CountAsync(e => e.IsActive),

                PublishedEvents = await repo.AllReadonly<Event>()
                    .CountAsync(e => e.IsActive && e.EventStatus == EventStatus.Published),

                TotalTicketsSold = await repo.AllReadonly<Ticket>()
                    .CountAsync(t => t.Status == TicketStatus.Purchased
                                  || t.Status == TicketStatus.Used),

                PendingApplications = await repo.AllReadonly<ApplicationForm>()
                    .CountAsync(a => a.Status == ApplicationStatus.Pending),

                TotalRevenue = await repo.AllReadonly<Payment>()
                    .Where(p => p.Status == Payment.PaymentStatus.Accepted)
                    .SumAsync(p => (decimal)p.Amount),

                ActiveVenues = await repo.AllReadonly<Venue>()
                    .CountAsync(v => v.IsActive),

                TodayRegistrations = await repo.AllReadonly<User>()
                    .CountAsync(u => u.CreatedAt.Date == today)
            };
        }

        public async Task<OrganizerDashboardViewModel> GetOrganizerDashboardAsync(string userId)
        {
            var organizerId = Guid.Parse(userId);
            var now = DateTime.UtcNow;

            return new OrganizerDashboardViewModel
            {
                MyEvents = await repo.AllReadonly<Event>()
                    .CountAsync(e => e.OrganizerId == organizerId && e.IsActive),

                PublishedEvents = await repo.AllReadonly<Event>()
                    .CountAsync(e => e.OrganizerId == organizerId && e.EventStatus == EventStatus.Published),

                DraftEvents = await repo.AllReadonly<Event>()
                    .CountAsync(e => e.OrganizerId == organizerId && e.EventStatus == EventStatus.Draft),

                TotalTicketsSold = await repo.AllReadonly<Event>()
                    .Where(e => e.OrganizerId == organizerId)
                    .SumAsync(e => e.TicketsSold),

                TotalRevenue = 0, // Can be calculated from payments in future

                UpcomingEventsCount = await repo.AllReadonly<Event>()
                    .CountAsync(e => e.OrganizerId == organizerId 
                               && e.IsActive 
                               && e.StartDateTime > now)
            };
        }
    }
}
