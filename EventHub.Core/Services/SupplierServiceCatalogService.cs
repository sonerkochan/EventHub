using EventHub.Core.Contracts;
using EventHub.Core.Models.Supplier;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Core.Services
{
    public class SupplierServiceCatalogService : ISupplierServiceCatalogService
    {
        private readonly IRepository repo;

        public SupplierServiceCatalogService(IRepository _repo)
        {
            repo = _repo;
        }

        public async Task<SupplierServiceSearchViewModel> SearchServicesAsync(string? searchTerm, string requesterId)
        {
            var normalizedSearchTerm = searchTerm?.Trim();
            var servicesQuery = repo.AllReadonly<SupplierService>();

            if (!string.IsNullOrWhiteSpace(normalizedSearchTerm))
            {
                servicesQuery = servicesQuery.Where(s =>
                    s.Name.Contains(normalizedSearchTerm) ||
                    (s.Description != null && s.Description.Contains(normalizedSearchTerm)));
            }

            var requestHistory = await repo.AllReadonly<ServiceRentalRequest>()
                .Where(r => r.RequesterId == requesterId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            var latestRequests = requestHistory
                .GroupBy(r => r.SupplierServiceId)
                .ToDictionary(g => g.Key, g => g.First());

            var services = await (
                from service in servicesQuery
                join supplier in repo.AllReadonly<User>()
                    on service.SupplierId equals supplier.Id into suppliers
                from supplier in suppliers.DefaultIfEmpty()
                orderby service.Name
                select new SupplierServiceCatalogItemViewModel
                {
                    Id = service.Id,
                    Name = service.Name,
                    Description = service.Description,
                    Price = service.Price,
                    SupplierName = supplier == null
                        ? "Unknown supplier"
                        : ((supplier.FirstName ?? "") + " " + (supplier.LastName ?? "")).Trim(),
                    SupplierEmail = supplier == null ? null : supplier.Email,
                    CreatedAt = service.CreatedAt
                })
                .ToListAsync();

            foreach (var service in services)
            {
                if (latestRequests.TryGetValue(service.Id, out var request))
                {
                    service.CurrentUserRequestId = request.Id;
                    service.CurrentUserRequestStatus = request.Status;
                }

                if (string.IsNullOrWhiteSpace(service.SupplierName))
                {
                    service.SupplierName = service.SupplierEmail ?? "Supplier";
                }
            }

            return new SupplierServiceSearchViewModel
            {
                SearchTerm = normalizedSearchTerm,
                Services = services
            };
        }

        public async Task<bool> RequestServiceAsync(int serviceId, string requesterId, string? message)
        {
            var service = await repo.AllReadonly<SupplierService>()
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null || service.SupplierId == requesterId)
            {
                return false;
            }

            var hasPendingRequest = await repo.AllReadonly<ServiceRentalRequest>()
                .AnyAsync(r => r.SupplierServiceId == serviceId
                    && r.RequesterId == requesterId
                    && r.Status == ServiceRentalRequestStatus.Pending);

            if (hasPendingRequest)
            {
                return false;
            }

            await repo.AddAsync(new ServiceRentalRequest
            {
                SupplierServiceId = serviceId,
                RequesterId = requesterId,
                Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim(),
                Status = ServiceRentalRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            });

            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ServiceRentalRequestListViewModel>> GetRequestsForSupplierAsync(string supplierId)
        {
            var requests = await repo.AllReadonly<ServiceRentalRequest>()
                .Where(r => r.SupplierService.SupplierId == supplierId)
                .Select(r => new ServiceRentalRequestListViewModel
                {
                    Id = r.Id,
                    SupplierServiceId = r.SupplierServiceId,
                    ServiceName = r.SupplierService.Name,
                    Price = r.SupplierService.Price,
                    RequesterName = ((r.Requester.FirstName ?? "") + " " + (r.Requester.LastName ?? "")).Trim(),
                    RequesterEmail = r.Requester.Email,
                    Message = r.Message,
                    ResponseComment = r.ResponseComment,
                    Status = r.Status,
                    RequestedAt = r.RequestedAt,
                    ReviewedAt = r.ReviewedAt
                })
                .OrderBy(r => r.Status)
                .ThenByDescending(r => r.RequestedAt)
                .ToListAsync();

            foreach (var request in requests)
            {
                if (string.IsNullOrWhiteSpace(request.RequesterName))
                {
                    request.RequesterName = request.RequesterEmail ?? "Requester";
                }
            }

            return requests;
        }

        public Task<bool> AcceptRequestAsync(int requestId, string supplierId, string reviewedById, string? responseComment)
            => ReviewRequestAsync(requestId, supplierId, reviewedById, ServiceRentalRequestStatus.Accepted, responseComment);

        public Task<bool> DeclineRequestAsync(int requestId, string supplierId, string reviewedById, string? responseComment)
            => ReviewRequestAsync(requestId, supplierId, reviewedById, ServiceRentalRequestStatus.Declined, responseComment);

        private async Task<bool> ReviewRequestAsync(
            int requestId,
            string supplierId,
            string reviewedById,
            ServiceRentalRequestStatus status,
            string? responseComment)
        {
            var request = await repo.All<ServiceRentalRequest>()
                .Include(r => r.SupplierService)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null ||
                request.SupplierService.SupplierId != supplierId ||
                request.Status != ServiceRentalRequestStatus.Pending)
            {
                return false;
            }

            request.Status = status;
            request.ReviewedById = reviewedById;
            request.ResponseComment = string.IsNullOrWhiteSpace(responseComment) ? null : responseComment.Trim();
            request.ReviewedAt = DateTime.UtcNow;

            repo.Update(request);
            await repo.SaveChangesAsync();
            return true;
        }
    }
}
