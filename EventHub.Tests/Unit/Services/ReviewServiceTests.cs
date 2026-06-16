using EventHub.Core.Models.Review;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using MockQueryable.Moq;
using Moq;

using DataReview = EventHub.Infrastructure.Data.Models.Review;
using static EventHub.Infrastructure.Data.Models.Review;
using static EventHub.Infrastructure.Data.Models.ReviewVote;

namespace EventHub.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class ReviewServiceTests
{
    [Fact]
    public async Task CreateAsync_AddsVisibleReviewForUser_ReturnsNewId()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        DataReview? added = null;

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<DataReview>()))
            .Callback<DataReview>(r => added = r)
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new ReviewService(repo.Object);

        var id = await service.CreateAsync(new CreateReviewViewModel
        {
            EventId = eventId,
            Rating = ReviewRating.FourStars,
            Title = "Great",
            Content = "Loved it"
        }, userId);

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(added);
        Assert.Equal(eventId, added!.EventId);
        Assert.Equal(userId, added.UserId);
        Assert.False(added.IsHidden);
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByEventAsync_ExcludesHiddenReviews_AndJoinsEventName()
    {
        var eventId = Guid.NewGuid();
        var reviews = new[]
        {
            new DataReview { Id = Guid.NewGuid(), EventId = eventId, Rating = ReviewRating.FiveStars, IsHidden = false, CreatedAt = new DateTime(2026, 5, 2) },
            new DataReview { Id = Guid.NewGuid(), EventId = eventId, Rating = ReviewRating.OneStar, IsHidden = true, CreatedAt = new DateTime(2026, 5, 3) },
            new DataReview { Id = Guid.NewGuid(), EventId = Guid.NewGuid(), Rating = ReviewRating.ThreeStars, IsHidden = false, CreatedAt = new DateTime(2026, 5, 1) }
        };
        var events = new[] { new Event { Id = eventId, EventName = "Festival" } };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<DataReview>()).Returns(reviews.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Event>()).Returns(events.AsQueryable().BuildMock());

        var service = new ReviewService(repo.Object);

        var result = (await service.GetByEventAsync(eventId)).ToList();

        Assert.Single(result);
        Assert.Equal("Festival", result[0].EventName);
        Assert.False(result[0].IsHidden);
    }

    [Fact]
    public async Task GetByIdAsync_PopulatesPositiveAndNegativeVoteCounts()
    {
        var reviewId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var reviews = new[] { new DataReview { Id = reviewId, EventId = eventId, Rating = ReviewRating.FourStars } };
        var events = new[] { new Event { Id = eventId, EventName = "Expo" } };
        var votes = new[]
        {
            new ReviewVote { Id = Guid.NewGuid(), ReviewId = reviewId, ReviewType = VoteType.Positive },
            new ReviewVote { Id = Guid.NewGuid(), ReviewId = reviewId, ReviewType = VoteType.Positive },
            new ReviewVote { Id = Guid.NewGuid(), ReviewId = reviewId, ReviewType = VoteType.Negative },
            new ReviewVote { Id = Guid.NewGuid(), ReviewId = Guid.NewGuid(), ReviewType = VoteType.Positive }
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<DataReview>()).Returns(reviews.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<Event>()).Returns(events.AsQueryable().BuildMock());
        repo.Setup(r => r.AllReadonly<ReviewVote>()).Returns(votes.AsQueryable().BuildMock());

        var service = new ReviewService(repo.Object);

        var result = await service.GetByIdAsync(reviewId);

        Assert.NotNull(result);
        Assert.Equal("Expo", result!.EventName);
        Assert.Equal(2, result.PositiveVotes);
        Assert.Equal(1, result.NegativeVotes);
    }

    [Fact]
    public async Task HideAsync_WhenReviewExists_SetsHiddenStateAndSaves()
    {
        var review = new DataReview { Id = Guid.NewGuid(), IsHidden = false };
        var moderatorId = Guid.NewGuid();
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<DataReview>()).Returns(new[] { review }.AsQueryable().BuildMock());
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new ReviewService(repo.Object);

        var result = await service.HideAsync(review.Id, moderatorId, "Inappropriate");

        Assert.True(result);
        Assert.True(review.IsHidden);
        Assert.Equal(moderatorId, review.HiddenBy);
        Assert.Equal("Inappropriate", review.HiddenReason);
        repo.Verify(r => r.Update(review), Times.Once);
    }

    [Fact]
    public async Task HideAsync_WhenReviewMissing_ReturnsFalseWithoutSaving()
    {
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<DataReview>()).Returns(Array.Empty<DataReview>().AsQueryable().BuildMock());

        var service = new ReviewService(repo.Object);

        Assert.False(await service.HideAsync(Guid.NewGuid(), Guid.NewGuid(), "x"));
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UnhideAsync_WhenReviewExists_ClearsHiddenState()
    {
        var review = new DataReview { Id = Guid.NewGuid(), IsHidden = true, HiddenBy = Guid.NewGuid(), HiddenReason = "spam" };
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<DataReview>()).Returns(new[] { review }.AsQueryable().BuildMock());
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new ReviewService(repo.Object);

        Assert.True(await service.UnhideAsync(review.Id));
        Assert.False(review.IsHidden);
        Assert.Equal(Guid.Empty, review.HiddenBy);
        Assert.Null(review.HiddenReason);
    }

    [Fact]
    public async Task DeleteAsync_WhenReviewMissing_ReturnsFalse()
    {
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<DataReview>(It.IsAny<object>())).ReturnsAsync((DataReview)null!);

        var service = new ReviewService(repo.Object);

        Assert.False(await service.DeleteAsync(Guid.NewGuid()));
        repo.Verify(r => r.Delete(It.IsAny<DataReview>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenReviewExists_DeletesAndSaves()
    {
        var review = new DataReview { Id = Guid.NewGuid() };
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.GetByIdAsync<DataReview>(review.Id)).ReturnsAsync(review);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new ReviewService(repo.Object);

        Assert.True(await service.DeleteAsync(review.Id));
        repo.Verify(r => r.Delete(review), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task VoteAsync_WhenNoExistingVote_AddsNewVote()
    {
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        ReviewVote? added = null;

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<ReviewVote>()).Returns(Array.Empty<ReviewVote>().AsQueryable().BuildMock());
        repo.Setup(r => r.AddAsync(It.IsAny<ReviewVote>()))
            .Callback<ReviewVote>(v => added = v)
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new ReviewService(repo.Object);

        Assert.True(await service.VoteAsync(reviewId, userId, isPositive: true));
        Assert.NotNull(added);
        Assert.Equal(VoteType.Positive, added!.ReviewType);
        repo.Verify(r => r.Update(It.IsAny<ReviewVote>()), Times.Never);
    }

    [Fact]
    public async Task VoteAsync_WhenExistingVote_UpdatesVoteType()
    {
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existing = new ReviewVote { Id = Guid.NewGuid(), ReviewId = reviewId, UserId = userId, ReviewType = VoteType.Positive };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<ReviewVote>()).Returns(new[] { existing }.AsQueryable().BuildMock());
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new ReviewService(repo.Object);

        Assert.True(await service.VoteAsync(reviewId, userId, isPositive: false));
        Assert.Equal(VoteType.Negative, existing.ReviewType);
        repo.Verify(r => r.Update(existing), Times.Once);
        repo.Verify(r => r.AddAsync(It.IsAny<ReviewVote>()), Times.Never);
    }

    [Fact]
    public async Task RemoveVoteAsync_WhenVoteMissing_ReturnsFalse()
    {
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<ReviewVote>()).Returns(Array.Empty<ReviewVote>().AsQueryable().BuildMock());

        var service = new ReviewService(repo.Object);

        Assert.False(await service.RemoveVoteAsync(Guid.NewGuid(), Guid.NewGuid()));
        repo.Verify(r => r.Delete(It.IsAny<ReviewVote>()), Times.Never);
    }

    [Fact]
    public async Task RemoveVoteAsync_WhenVoteExists_DeletesAndSaves()
    {
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existing = new ReviewVote { Id = Guid.NewGuid(), ReviewId = reviewId, UserId = userId };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<ReviewVote>()).Returns(new[] { existing }.AsQueryable().BuildMock());
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new ReviewService(repo.Object);

        Assert.True(await service.RemoveVoteAsync(reviewId, userId));
        repo.Verify(r => r.Delete(existing), Times.Once);
    }
}
