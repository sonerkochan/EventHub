using EventHub.Core.Models.Review;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventHub.Core.Contracts
{
    public interface IReviewService
    {
        Task<Guid> CreateAsync(CreateReviewViewModel model, Guid userId);
        Task<IEnumerable<ReviewListViewModel>> GetAllAsync();
        Task<IEnumerable<ReviewListViewModel>> GetByEventAsync(Guid eventId);
        Task<IEnumerable<ReviewListViewModel>> GetByUserAsync(Guid userId);
        Task<ReviewDetailViewModel?> GetByIdAsync(Guid id);
        Task<bool> HideAsync(Guid id, Guid hiddenBy, string reason);
        Task<bool> UnhideAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> VoteAsync(Guid reviewId, Guid userId, bool isPositive);
        Task<bool> RemoveVoteAsync(Guid reviewId, Guid userId);
    }
}
