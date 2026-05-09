namespace EventHub.Core.Models.Supplier
{
    public class SupplierServiceSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public IEnumerable<SupplierServiceCatalogItemViewModel> Services { get; set; } =
            new List<SupplierServiceCatalogItemViewModel>();
    }
}
