using EventHub.Core.Models.Supplier;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Tests.Unit.Supplier;

public class SupplierViewModelTests
{
    [Fact]
    public void ServiceRentalRequestListViewModel_ShouldInitializeWithDefaultValues()
    {
        var model = new ServiceRentalRequestListViewModel();

        Assert.Equal(0, model.Id);
        Assert.Equal(0, model.SupplierServiceId);
        Assert.Null(model.Price);
        Assert.Equal(string.Empty, model.PriceText);
        Assert.Null(model.RequesterEmail);
        Assert.Null(model.Message);
        Assert.Null(model.ResponseComment);
        Assert.Null(model.ReviewedAt);
    }

    [Fact]
    public void ServiceRentalRequestListViewModel_ShouldAllowSettingProperties()
    {
        var requestedAt = DateTime.UtcNow;
        var reviewedAt = requestedAt.AddHours(1);

        var model = new ServiceRentalRequestListViewModel
        {
            Id = 1,
            SupplierServiceId = 10,
            ServiceName = "Sound System",
            Price = 250.00m,
            PriceText = "250.00 лв.",
            RequesterName = "Ivan Ivanov",
            RequesterEmail = "ivan@example.com",
            Message = "Need equipment for event",
            ResponseComment = "Accepted",
            Status = ServiceRentalRequestStatus.Accepted,
            RequestedAt = requestedAt,
            ReviewedAt = reviewedAt
        };

        Assert.Equal(1, model.Id);
        Assert.Equal(10, model.SupplierServiceId);
        Assert.Equal("Sound System", model.ServiceName);
        Assert.Equal(250.00m, model.Price);
        Assert.Equal("250.00 лв.", model.PriceText);
        Assert.Equal("Ivan Ivanov", model.RequesterName);
        Assert.Equal("ivan@example.com", model.RequesterEmail);
        Assert.Equal("Need equipment for event", model.Message);
        Assert.Equal("Accepted", model.ResponseComment);
        Assert.Equal(ServiceRentalRequestStatus.Accepted, model.Status);
        Assert.Equal(requestedAt, model.RequestedAt);
        Assert.Equal(reviewedAt, model.ReviewedAt);
    }

    [Fact]
    public void SupplierServiceCatalogItemViewModel_ShouldInitializeWithDefaultValues()
    {
        var model = new SupplierServiceCatalogItemViewModel();

        Assert.Equal(0, model.Id);
        Assert.Null(model.Description);
        Assert.Null(model.Price);
        Assert.Equal(string.Empty, model.PriceText);
        Assert.Null(model.SupplierEmail);
        Assert.Null(model.CurrentUserRequestId);
        Assert.Null(model.CurrentUserRequestStatus);
    }

    [Fact]
    public void SupplierServiceCatalogItemViewModel_ShouldAllowSettingProperties()
    {
        var createdAt = DateTime.UtcNow;

        var model = new SupplierServiceCatalogItemViewModel
        {
            Id = 5,
            Name = "Lighting Equipment",
            Description = "Professional lighting for events",
            Price = 500.00m,
            PriceText = "500.00 лв.",
            SupplierName = "Event Supplier Ltd.",
            SupplierEmail = "supplier@example.com",
            CreatedAt = createdAt,
            CurrentUserRequestId = 100,
            CurrentUserRequestStatus = ServiceRentalRequestStatus.Pending
        };

        Assert.Equal(5, model.Id);
        Assert.Equal("Lighting Equipment", model.Name);
        Assert.Equal("Professional lighting for events", model.Description);
        Assert.Equal(500.00m, model.Price);
        Assert.Equal("500.00 лв.", model.PriceText);
        Assert.Equal("Event Supplier Ltd.", model.SupplierName);
        Assert.Equal("supplier@example.com", model.SupplierEmail);
        Assert.Equal(createdAt, model.CreatedAt);
        Assert.Equal(100, model.CurrentUserRequestId);
        Assert.Equal(ServiceRentalRequestStatus.Pending, model.CurrentUserRequestStatus);
    }

    [Fact]
    public void SupplierServiceSearchViewModel_ShouldInitializeServicesAsEmptyList()
    {
        var model = new SupplierServiceSearchViewModel();

        Assert.Null(model.SearchTerm);
        Assert.NotNull(model.Services);
        Assert.Empty(model.Services);
    }

    [Fact]
    public void SupplierServiceSearchViewModel_ShouldAllowSettingSearchTermAndServices()
    {
        var services = new List<SupplierServiceCatalogItemViewModel>
        {
            new SupplierServiceCatalogItemViewModel
            {
                Id = 1,
                Name = "Catering",
                SupplierName = "Food Supplier Ltd.",
                PriceText = "По договаряне"
            },
            new SupplierServiceCatalogItemViewModel
            {
                Id = 2,
                Name = "Sound System",
                SupplierName = "Audio Supplier Ltd.",
                PriceText = "300.00 лв."
            }
        };

        var model = new SupplierServiceSearchViewModel
        {
            SearchTerm = "sound",
            Services = services
        };

        Assert.Equal("sound", model.SearchTerm);
        Assert.Equal(2, model.Services.Count());
        Assert.Contains(model.Services, s => s.Name == "Sound System");
        Assert.Contains(model.Services, s => s.Name == "Catering");
    }
}