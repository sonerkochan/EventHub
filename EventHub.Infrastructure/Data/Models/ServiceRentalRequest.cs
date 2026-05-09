using System.ComponentModel.DataAnnotations;

namespace EventHub.Infrastructure.Data.Models
{
    public class ServiceRentalRequest
    {
        [Key]
        public int Id { get; set; }

        public int SupplierServiceId { get; set; }
        public SupplierService SupplierService { get; set; } = null!;

        public string RequesterId { get; set; } = null!;
        public User Requester { get; set; } = null!;

        public ServiceRentalRequestStatus Status { get; set; } = ServiceRentalRequestStatus.Pending;

        public string? Message { get; set; }

        public string? ReviewedById { get; set; }
        public User? ReviewedBy { get; set; }

        public string? ResponseComment { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }

    public enum ServiceRentalRequestStatus
    {
        Pending = 1,
        Accepted = 2,
        Declined = 3
    }
}
