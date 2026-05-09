using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Models.Supplier
{
    public class ServiceRentalRequestListViewModel
    {
        public int Id { get; set; }
        public int SupplierServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public decimal? Price { get; set; }
        public string RequesterName { get; set; } = null!;
        public string? RequesterEmail { get; set; }
        public string? Message { get; set; }
        public string? ResponseComment { get; set; }
        public ServiceRentalRequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
