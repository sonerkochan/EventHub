using EventHub.Core.Models.Refund;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventHub.Core.Contracts
{
    public interface IRefundService
    {
        Task<Guid> RequestAsync(CreateRefundViewModel model, Guid requestedBy);
        Task<IEnumerable<RefundListViewModel>> GetAllAsync();
        Task<IEnumerable<RefundListViewModel>> GetByUserAsync(Guid userId);
        Task<RefundDetailViewModel?> GetByIdAsync(Guid id);
        Task<bool> ApproveAsync(Guid id, Guid processedBy);
        Task<bool> RejectAsync(Guid id, Guid processedBy);
        Task<bool> CompleteAsync(Guid id);
    }
}
