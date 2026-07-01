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
        Task<RefundOperationResult> RequestTicketRefundAsync(Guid ticketId, Guid requestedBy, string? reason);
        Task<IEnumerable<RefundListViewModel>> GetRefundsForUserAsync(Guid userId);
        Task<IEnumerable<OrganizerRefundListItemViewModel>> GetRefundsForOrganizerAsync(
            Guid organizerId,
            EventHub.Infrastructure.Data.Models.Refund.RefundStatus? statusFilter = null);
        Task<RefundOperationResult> ApproveTicketRefundAsync(Guid refundId, Guid organizerId);
        Task<RefundOperationResult> RejectTicketRefundAsync(Guid refundId, Guid organizerId, string? comment);
    }
}
