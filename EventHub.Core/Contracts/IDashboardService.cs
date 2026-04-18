using EventHub.Core.Models.Dashboard;
using System.Threading.Tasks;

namespace EventHub.Core.Contracts
{
    public interface IDashboardService
    {
        Task<AdminDashboardViewModel> GetAdminDashboardAsync();
        Task<OrganizerDashboardViewModel> GetOrganizerDashboardAsync(string userId);
    }
}
