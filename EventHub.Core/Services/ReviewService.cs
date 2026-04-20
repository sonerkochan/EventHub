using EventHub.Core.Contracts;
using EventHub.Core.Models.Review;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DataReview = EventHub.Infrastructure.Data.Models.Review;

namespace EventHub.Core.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IRepository repo;

        public ReviewService(IRepository _repo)
        {
            repo = _repo;
        }

        public async Task<Guid> CreateAsync(CreateReviewViewModel model, Guid userId)
        {
            var entity = new DataReview
            {
                Id = Guid.NewGuid(),
                EventId = model.EventId,
                UserId = userId,
                Rating = model.Rating,
                Title = model.Title,
                Content = model.Content,
                IsHidden = false,
                HiddenBy = Guid.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<IEnumerable<ReviewListViewModel>> GetAllAsync()
        {
            return await BuildListQuery(repo.AllReadonly<DataReview>())
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewListViewModel>> GetByEventAsync(Guid eventId)
        {
            return await BuildListQuery(
                    repo.AllReadonly<DataReview>().Where(r => r.EventId == eventId && !r.IsHidden))
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewListViewModel>> GetByUserAsync(Guid userId)
        {
            return await BuildListQuery(
                    repo.AllReadonly<DataReview>().Where(r => r.UserId == userId))
                .ToListAsync();
        }

        public async Task<ReviewDetailViewModel?> GetByIdAsync(Guid id)
        {
            var review = await repo.AllReadonly<DataReview>()
                .Where(r => r.Id == id)
                .Join(
                    repo.AllReadonly<Event>(),
                    r => r.EventId,
                    e => e.Id,
                    (r, e) => new ReviewDetailViewModel
                    {
                        Id = r.Id,
                        EventId = r.EventId,
                        EventName = e.EventName,
                        UserId = r.UserId,
                        Rating = r.Rating,
                        Title = r.Title,
                        Content = r.Content,
                        IsHidden = r.IsHidden,
                        HiddenReason = r.HiddenReason,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                .FirstOrDefaultAsync();

            if (review != null)
            {
                review.PositiveVotes = await repo.AllReadonly<ReviewVote>()
                    .CountAsync(v => v.ReviewId == id && v.ReviewType == ReviewVote.VoteType.Positive);
                review.NegativeVotes = await repo.AllReadonly<ReviewVote>()
                    .CountAsync(v => v.ReviewId == id && v.ReviewType == ReviewVote.VoteType.Negative);
            }

            return review;
        }

        public async Task<bool> HideAsync(Guid id, Guid hiddenBy, string reason)
        {
            var entity = await repo.All<DataReview>()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null) return false;

            entity.IsHidden = true;
            entity.HiddenBy = hiddenBy;
            entity.HiddenReason = reason;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnhideAsync(Guid id)
        {
            var entity = await repo.All<DataReview>()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null) return false;

            entity.IsHidden = false;
            entity.HiddenBy = Guid.Empty;
            entity.HiddenReason = null;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await repo.GetByIdAsync<DataReview>(id);
            if (entity == null) return false;

            repo.Delete(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VoteAsync(Guid reviewId, Guid userId, bool isPositive)
        {
            var existing = await repo.All<ReviewVote>()
                .FirstOrDefaultAsync(v => v.ReviewId == reviewId && v.UserId == userId);

            var voteType = isPositive
                ? ReviewVote.VoteType.Positive
                : ReviewVote.VoteType.Negative;

            if (existing != null)
            {
                existing.ReviewType = voteType;
                existing.UpdatedAt = DateTime.UtcNow;
                repo.Update(existing);
            }
            else
            {
                var vote = new ReviewVote
                {
                    Id = Guid.NewGuid(),
                    ReviewId = reviewId,
                    UserId = userId,
                    ReviewType = voteType,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await repo.AddAsync(vote);
            }

            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveVoteAsync(Guid reviewId, Guid userId)
        {
            var existing = await repo.All<ReviewVote>()
                .FirstOrDefaultAsync(v => v.ReviewId == reviewId && v.UserId == userId);

            if (existing == null) return false;

            repo.Delete(existing);
            await repo.SaveChangesAsync();
            return true;
        }

        private IQueryable<ReviewListViewModel> BuildListQuery(IQueryable<DataReview> source)
        {
            return source
                .Join(
                    repo.AllReadonly<Event>(),
                    r => r.EventId,
                    e => e.Id,
                    (r, e) => new ReviewListViewModel
                    {
                        Id = r.Id,
                        EventId = r.EventId,
                        EventName = e.EventName,
                        UserId = r.UserId,
                        Rating = r.Rating,
                        Title = r.Title,
                        IsHidden = r.IsHidden,
                        CreatedAt = r.CreatedAt
                    })
                .OrderByDescending(r => r.CreatedAt);
        }
    }
}
