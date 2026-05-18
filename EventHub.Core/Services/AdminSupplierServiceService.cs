using EventHub.Core.Contracts;
using EventHub.Core.Models.Admin;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Core.Services
{
    public class AdminSupplierServiceService : IAdminSupplierServiceService
    {
        private readonly IRepository repo;
        private readonly ICurrencyDisplayService currencyDisplayService;

        public AdminSupplierServiceService(
            IRepository _repo,
            ICurrencyDisplayService _currencyDisplayService)
        {
            repo = _repo;
            currencyDisplayService = _currencyDisplayService;
        }

        public async Task<IEnumerable<AdminSupplierServiceListItem>> GetAllAsync(string? statusFilter = null, string? searchTerm = null)
        {
            var query = repo.AllReadonly<SupplierService>().IgnoreQueryFilters();

            if (string.Equals(statusFilter, "visible", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => !s.IsDeleted);
            }
            else if (string.Equals(statusFilter, "hidden", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(s => s.Name.Contains(term)
                                      || (s.Description != null && s.Description.Contains(term)));
            }

            var rows = await (
                from service in query
                join supplier in repo.AllReadonly<User>()
                    on service.SupplierId equals supplier.Id into suppliers
                from supplier in suppliers.DefaultIfEmpty()
                orderby service.IsDeleted, service.CreatedAt descending
                select new AdminSupplierServiceListItem
                {
                    Id = service.Id,
                    Name = service.Name,
                    Description = service.Description,
                    Price = service.Price,
                    SupplierId = service.SupplierId,
                    SupplierName = supplier == null
                        ? "Unknown supplier"
                        : ((supplier.FirstName ?? "") + " " + (supplier.LastName ?? "")).Trim(),
                    SupplierEmail = supplier == null ? null : supplier.Email,
                    CreatedAt = service.CreatedAt,
                    UpdatedAt = service.UpdatedAt,
                    IsHidden = service.IsDeleted,
                    HiddenAt = service.DeletedAt
                }).ToListAsync();

            if (rows.Count == 0) return rows;

            var serviceIds = rows.Select(r => r.Id).ToList();

            var requestStats = await repo.AllReadonly<ServiceRentalRequest>()
                .Where(r => serviceIds.Contains(r.SupplierServiceId))
                .GroupBy(r => r.SupplierServiceId)
                .Select(g => new
                {
                    ServiceId = g.Key,
                    Pending = g.Count(r => r.Status == ServiceRentalRequestStatus.Pending),
                    Total = g.Count()
                })
                .ToListAsync();
            var statsById = requestStats.ToDictionary(s => s.ServiceId);

            foreach (var row in rows)
            {
                if (statsById.TryGetValue(row.Id, out var stats))
                {
                    row.PendingRequestCount = stats.Pending;
                    row.TotalRequestCount = stats.Total;
                }

                if (string.IsNullOrWhiteSpace(row.SupplierName))
                {
                    row.SupplierName = row.SupplierEmail ?? "Supplier";
                }

                row.PriceText = row.Price.HasValue
                    ? (await currencyDisplayService.FormatAsync(row.Price.Value)).Text
                    : string.Empty;
            }

            return rows;
        }

        public async Task<bool> HideAsync(int serviceId)
        {
            var service = await repo.All<SupplierService>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null || service.IsDeleted) return false;

            service.IsDeleted = true;
            service.DeletedAt = DateTime.UtcNow;
            service.UpdatedAt = DateTime.UtcNow;

            repo.Update(service);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnhideAsync(int serviceId)
        {
            var service = await repo.All<SupplierService>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null || !service.IsDeleted) return false;

            service.IsDeleted = false;
            service.DeletedAt = null;
            service.UpdatedAt = DateTime.UtcNow;

            repo.Update(service);
            await repo.SaveChangesAsync();
            return true;
        }
    }
}
