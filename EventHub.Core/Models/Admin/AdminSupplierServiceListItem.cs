using System;

namespace EventHub.Core.Models.Admin
{
    public class AdminSupplierServiceListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string PriceText { get; set; } = string.Empty;
        public string? SupplierId { get; set; }
        public string SupplierName { get; set; } = null!;
        public string? SupplierEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsHidden { get; set; }
        public DateTime? HiddenAt { get; set; }
        public int PendingRequestCount { get; set; }
        public int TotalRequestCount { get; set; }
    }
}
