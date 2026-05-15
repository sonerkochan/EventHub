using EventHub.Core.Models.Admin;

namespace EventHub.Core.Contracts
{
    public interface IAdminSupplierServiceService
    {
        Task<IEnumerable<AdminSupplierServiceListItem>> GetAllAsync(string? statusFilter = null, string? searchTerm = null);
        Task<bool> HideAsync(int serviceId);
        Task<bool> UnhideAsync(int serviceId);
    }
}
