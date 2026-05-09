using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Supplier
{
    public class SupplierServiceCatalogItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string SupplierName { get; set; } = null!;
        public string? SupplierEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CurrentUserRequestId { get; set; }
        public ServiceRentalRequestStatus? CurrentUserRequestStatus { get; set; }
    }
}
