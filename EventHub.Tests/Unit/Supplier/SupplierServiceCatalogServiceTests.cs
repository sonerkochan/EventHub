using EventHub.Core.Contracts;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using MockQueryable.Moq;
using Moq;

namespace EventHub.Tests.Unit.Supplier;

[Trait("Category", "E2E")]
public class SupplierServiceCatalogServiceTests
{
    private readonly Mock<IRepository> repoMock = new();
    private readonly Mock<ICurrencyDisplayService> currencyMock = new();

    private SupplierServiceCatalogService CreateService()
        => new SupplierServiceCatalogService(
            repoMock.Object,
            currencyMock.Object);

    [Fact]
    public async Task RequestServiceAsync_WhenServiceDoesNotExist_ShouldReturnFalse()
    {
        var services = new List<SupplierService>().AsQueryable().BuildMock();

        repoMock
            .Setup(r => r.AllReadonly<SupplierService>())
            .Returns(services);

        var service = CreateService();

        var result = await service.RequestServiceAsync(999, "user-1", "message");

        Assert.False(result);
        repoMock.Verify(r => r.AddAsync(It.IsAny<ServiceRentalRequest>()), Times.Never);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task RequestServiceAsync_WhenRequesterIsSupplier_ShouldReturnFalse()
    {
        var services = new List<SupplierService>
        {
            new SupplierService
            {
                Id = 1,
                Name = "Sound System",
                SupplierId = "supplier-1"
            }
        }.AsQueryable().BuildMock();

        repoMock
            .Setup(r => r.AllReadonly<SupplierService>())
            .Returns(services);

        var service = CreateService();

        var result = await service.RequestServiceAsync(1, "supplier-1", "message");

        Assert.False(result);
        repoMock.Verify(r => r.AddAsync(It.IsAny<ServiceRentalRequest>()), Times.Never);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task RequestServiceAsync_WhenPendingRequestAlreadyExists_ShouldReturnFalse()
    {
        var services = new List<SupplierService>
        {
            new SupplierService
            {
                Id = 1,
                Name = "Lighting",
                SupplierId = "supplier-1"
            }
        }.AsQueryable().BuildMock();

        var requests = new List<ServiceRentalRequest>
        {
            new ServiceRentalRequest
            {
                Id = 10,
                SupplierServiceId = 1,
                RequesterId = "user-1",
                Status = ServiceRentalRequestStatus.Pending
            }
        }.AsQueryable().BuildMock();

        repoMock
            .Setup(r => r.AllReadonly<SupplierService>())
            .Returns(services);

        repoMock
            .Setup(r => r.AllReadonly<ServiceRentalRequest>())
            .Returns(requests);

        var service = CreateService();

        var result = await service.RequestServiceAsync(1, "user-1", "message");

        Assert.False(result);
        repoMock.Verify(r => r.AddAsync(It.IsAny<ServiceRentalRequest>()), Times.Never);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task RequestServiceAsync_WhenValidRequest_ShouldCreatePendingRequestAndReturnTrue()
    {
        var services = new List<SupplierService>
        {
            new SupplierService
            {
                Id = 1,
                Name = "Catering",
                SupplierId = "supplier-1"
            }
        }.AsQueryable().BuildMock();

        var requests = new List<ServiceRentalRequest>().AsQueryable().BuildMock();

        repoMock
            .Setup(r => r.AllReadonly<SupplierService>())
            .Returns(services);

        repoMock
            .Setup(r => r.AllReadonly<ServiceRentalRequest>())
            .Returns(requests);

        ServiceRentalRequest? addedRequest = null;

        repoMock
            .Setup(r => r.AddAsync(It.IsAny<ServiceRentalRequest>()))
            .Callback<ServiceRentalRequest>(r => addedRequest = r)
            .Returns(Task.CompletedTask);

        repoMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = CreateService();

        var result = await service.RequestServiceAsync(1, "user-1", "  Need this service  ");

        Assert.True(result);
        Assert.NotNull(addedRequest);
        Assert.Equal(1, addedRequest!.SupplierServiceId);
        Assert.Equal("user-1", addedRequest.RequesterId);
        Assert.Equal("Need this service", addedRequest.Message);
        Assert.Equal(ServiceRentalRequestStatus.Pending, addedRequest.Status);

        repoMock.Verify(r => r.AddAsync(It.IsAny<ServiceRentalRequest>()), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AcceptRequestAsync_WhenRequestIsValid_ShouldAcceptRequest()
    {
        var requests = new List<ServiceRentalRequest>
        {
            new ServiceRentalRequest
            {
                Id = 1,
                SupplierServiceId = 10,
                RequesterId = "user-1",
                Status = ServiceRentalRequestStatus.Pending,
                SupplierService = new SupplierService
                {
                    Id = 10,
                    SupplierId = "supplier-1",
                    Name = "Lighting"
                }
            }
        }.AsQueryable().BuildMock();

        repoMock
            .Setup(r => r.All<ServiceRentalRequest>())
            .Returns(requests);

        repoMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = CreateService();

        var result = await service.AcceptRequestAsync(
            1,
            "supplier-1",
            "reviewer-1",
            "Accepted successfully");

        Assert.True(result);

        var request = requests.First();

        Assert.Equal(ServiceRentalRequestStatus.Accepted, request.Status);
        Assert.Equal("reviewer-1", request.ReviewedById);
        Assert.Equal("Accepted successfully", request.ResponseComment);
        Assert.NotNull(request.ReviewedAt);

        repoMock.Verify(r => r.Update(It.IsAny<ServiceRentalRequest>()), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeclineRequestAsync_WhenRequestIsValid_ShouldDeclineRequest()
    {
        var requests = new List<ServiceRentalRequest>
        {
            new ServiceRentalRequest
            {
                Id = 2,
                SupplierServiceId = 20,
                RequesterId = "user-2",
                Status = ServiceRentalRequestStatus.Pending,
                SupplierService = new SupplierService
                {
                    Id = 20,
                    SupplierId = "supplier-2",
                    Name = "Catering"
                }
            }
        }.AsQueryable().BuildMock();

        repoMock
            .Setup(r => r.All<ServiceRentalRequest>())
            .Returns(requests);

        repoMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = CreateService();

        var result = await service.DeclineRequestAsync(
            2,
            "supplier-2",
            "reviewer-2",
            "Declined");

        Assert.True(result);

        var request = requests.First();

        Assert.Equal(ServiceRentalRequestStatus.Declined, request.Status);
        Assert.Equal("reviewer-2", request.ReviewedById);
        Assert.Equal("Declined", request.ResponseComment);
        Assert.NotNull(request.ReviewedAt);

        repoMock.Verify(r => r.Update(It.IsAny<ServiceRentalRequest>()), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
