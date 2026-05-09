using EventHub.Core.Models.Supplier;

namespace EventHub.Core.Contracts
{
    public interface ISupplierServiceCatalogService
    {
        Task<SupplierServiceSearchViewModel> SearchServicesAsync(string? searchTerm, string requesterId);
        Task<bool> RequestServiceAsync(int serviceId, string requesterId, string? message);
        Task<IEnumerable<ServiceRentalRequestListViewModel>> GetRequestsForSupplierAsync(string supplierId);
        Task<bool> AcceptRequestAsync(int requestId, string supplierId, string reviewedById, string? responseComment);
        Task<bool> DeclineRequestAsync(int requestId, string supplierId, string reviewedById, string? responseComment);
    }
}
