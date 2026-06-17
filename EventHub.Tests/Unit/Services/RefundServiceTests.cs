using EventHub.Core.Models.Refund;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using MockQueryable.Moq;
using Moq;

using DataRefund = EventHub.Infrastructure.Data.Models.Refund;
using static EventHub.Infrastructure.Data.Models.Refund;

namespace EventHub.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class RefundServiceTests
{
    [Fact]
    public async Task RequestAsync_CreatesPendingRefund_ReturnsNewId()
    {
        var paymentId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();
        DataRefund? added = null;

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<DataRefund>()))
            .Callback<DataRefund>(r => added = r)
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var service = new RefundService(repo.Object);

        var id = await service.RequestAsync(new CreateRefundViewModel
        {
            PaymentId = paymentId,
            Amount = 42.50f,
            Reason = "Event cancelled"
        }, requestedBy);

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(added);
        Assert.Equal(paymentId, added!.PaymentId);
        Assert.Equal(requestedBy, added.RequestedBy);
        Assert.Equal(RefundStatus.Pending, added.Status);
        Assert.Equal("EUR", added.Currency);
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyUsersRefunds_OrderedByCreatedAtDesc()
    {
        var userId = Guid.NewGuid();
        var refunds = new[]
        {
            new DataRefund { Id = Guid.NewGuid(), RequestedBy = userId, CreatedAt = new DateTime(2026, 5, 1) },
            new DataRefund { Id = Guid.NewGuid(), RequestedBy = userId, CreatedAt = new DateTime(2026, 5, 3) },
            new DataRefund { Id = Guid.NewGuid(), RequestedBy = Guid.NewGuid(), CreatedAt = new DateTime(2026, 5, 5) }
        };

        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<DataRefund>()).Returns(refunds.AsQueryable().BuildMock());

        var service = new RefundService(repo.Object);

        var result = (await service.GetByUserAsync(userId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(userId, r.RequestedBy));
        Assert.True(result[0].CreatedAt > result[1].CreatedAt);
    }

    [Fact]
    public async Task ApproveAsync_WhenPending_TransitionsToApproved()
    {
        var refund = new DataRefund { Id = Guid.NewGuid(), Status = RefundStatus.Pending };
        var processedBy = Guid.NewGuid();
        var repo = CreateRepoWith(refund);

        var service = new RefundService(repo.Object);

        var result = await service.ApproveAsync(refund.Id, processedBy);

        Assert.True(result);
        Assert.Equal(RefundStatus.Approved, refund.Status);
        Assert.Equal(processedBy, refund.ProcessedBy);
        repo.Verify(r => r.Update(refund), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_WhenNotPending_ReturnsFalseWithoutSaving()
    {
        var refund = new DataRefund { Id = Guid.NewGuid(), Status = RefundStatus.Approved };
        var repo = CreateRepoWith(refund);

        var service = new RefundService(repo.Object);

        Assert.False(await service.ApproveAsync(refund.Id, Guid.NewGuid()));
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task RejectAsync_WhenPending_TransitionsToRejected()
    {
        var refund = new DataRefund { Id = Guid.NewGuid(), Status = RefundStatus.Pending };
        var repo = CreateRepoWith(refund);

        var service = new RefundService(repo.Object);

        Assert.True(await service.RejectAsync(refund.Id, Guid.NewGuid()));
        Assert.Equal(RefundStatus.Rejected, refund.Status);
    }

    [Fact]
    public async Task CompleteAsync_WhenApproved_TransitionsToCompleted()
    {
        var refund = new DataRefund { Id = Guid.NewGuid(), Status = RefundStatus.Approved };
        var repo = CreateRepoWith(refund);

        var service = new RefundService(repo.Object);

        Assert.True(await service.CompleteAsync(refund.Id));
        Assert.Equal(RefundStatus.Completed, refund.Status);
        repo.Verify(r => r.Update(refund), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_WhenNotApproved_ReturnsFalse()
    {
        var refund = new DataRefund { Id = Guid.NewGuid(), Status = RefundStatus.Pending };
        var repo = CreateRepoWith(refund);

        var service = new RefundService(repo.Object);

        Assert.False(await service.CompleteAsync(refund.Id));
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_MapsDetailFields()
    {
        var refund = new DataRefund
        {
            Id = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            RequestedBy = Guid.NewGuid(),
            ProcessedBy = Guid.NewGuid(),
            Amount = 15.5f,
            Currency = "EUR",
            Reason = "Duplicate charge",
            Status = RefundStatus.Approved
        };
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.AllReadonly<DataRefund>()).Returns(new[] { refund }.AsQueryable().BuildMock());

        var service = new RefundService(repo.Object);

        var result = await service.GetByIdAsync(refund.Id);

        Assert.NotNull(result);
        Assert.Equal(refund.Id, result!.Id);
        Assert.Equal(refund.PaymentId, result.PaymentId);
        Assert.Equal("Duplicate charge", result.Reason);
        Assert.Equal(RefundStatus.Approved, result.Status);
    }

    private static Mock<IRepository> CreateRepoWith(DataRefund refund)
    {
        var repo = new Mock<IRepository>();
        repo.Setup(r => r.All<DataRefund>()).Returns(new[] { refund }.AsQueryable().BuildMock());
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        return repo;
    }
}
