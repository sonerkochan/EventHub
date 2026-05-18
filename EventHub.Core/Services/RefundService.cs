using EventHub.Core.Contracts;
using EventHub.Core.Models.Refund;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DataRefund = EventHub.Infrastructure.Data.Models.Refund;

namespace EventHub.Core.Services
{
    public class RefundService : IRefundService
    {
        private readonly IRepository repo;

        public RefundService(IRepository _repo)
        {
            repo = _repo;
        }

        public async Task<Guid> RequestAsync(CreateRefundViewModel model, Guid requestedBy)
        {
            var entity = new DataRefund
            {
                Id = Guid.NewGuid(),
                PaymentId = model.PaymentId,
                RequestedBy = requestedBy,
                ProcessedBy = Guid.Empty,
                StripeRefundId = null,
                Amount = model.Amount,
                Currency = "EUR",
                Reason = model.Reason,
                Status = DataRefund.RefundStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<IEnumerable<RefundListViewModel>> GetAllAsync()
        {
            return await repo.AllReadonly<DataRefund>()
                .Select(r => new RefundListViewModel
                {
                    Id = r.Id,
                    PaymentId = r.PaymentId,
                    RequestedBy = r.RequestedBy,
                    Amount = r.Amount,
                    Currency = r.Currency,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefundListViewModel>> GetByUserAsync(Guid userId)
        {
            return await repo.AllReadonly<DataRefund>()
                .Where(r => r.RequestedBy == userId)
                .Select(r => new RefundListViewModel
                {
                    Id = r.Id,
                    PaymentId = r.PaymentId,
                    RequestedBy = r.RequestedBy,
                    Amount = r.Amount,
                    Currency = r.Currency,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<RefundDetailViewModel?> GetByIdAsync(Guid id)
        {
            return await repo.AllReadonly<DataRefund>()
                .Where(r => r.Id == id)
                .Select(r => new RefundDetailViewModel
                {
                    Id = r.Id,
                    PaymentId = r.PaymentId,
                    RequestedBy = r.RequestedBy,
                    ProcessedBy = r.ProcessedBy,
                    Amount = r.Amount,
                    Currency = r.Currency,
                    Reason = r.Reason,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    ProcessedAt = r.ProcessedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ApproveAsync(Guid id, Guid processedBy)
        {
            var entity = await repo.All<DataRefund>()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null || entity.Status != DataRefund.RefundStatus.Pending)
                return false;

            entity.Status = DataRefund.RefundStatus.Approved;
            entity.ProcessedBy = processedBy;
            entity.ProcessedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(Guid id, Guid processedBy)
        {
            var entity = await repo.All<DataRefund>()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null || entity.Status != DataRefund.RefundStatus.Pending)
                return false;

            entity.Status = DataRefund.RefundStatus.Rejected;
            entity.ProcessedBy = processedBy;
            entity.ProcessedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteAsync(Guid id)
        {
            var entity = await repo.All<DataRefund>()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null || entity.Status != DataRefund.RefundStatus.Approved)
                return false;

            entity.Status = DataRefund.RefundStatus.Completed;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }
    }
}
