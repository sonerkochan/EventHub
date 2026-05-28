using System.Security.Claims;
using EventHub.Areas.Admin.Controllers;
using EventHub.Core.Contracts;
using EventHub.Core.Models.Venue;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventHub.Tests.Unit.Admin;

public class VenuesControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithVenues()
    {
        var venues = new List<VenueListViewModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Main Hall",
                City = "Sofia",
                Country = "Bulgaria",
                Address = "1 Main Street",
                Latitude = 42.6977f,
                Longitude = 23.3219f
            }
        };
        var venueService = new Mock<IVenueService>();
        venueService
            .Setup(s => s.GetAllVenuesAsync())
            .ReturnsAsync(venues);
        var controller = new VenuesController(venueService.Object);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<VenueListViewModel>>(viewResult.Model);
        Assert.Same(venues, model);
        venueService.Verify(s => s.GetAllVenuesAsync(), Times.Once);
    }

    [Fact]
    public void CreatePartial_ReturnsCreateModalPartialWithAddVenueViewModel()
    {
        var venueService = new Mock<IVenueService>();
        var controller = new VenuesController(venueService.Object);

        var result = controller.CreatePartial();

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_CreateModal", partial.ViewName);
        Assert.IsType<AddVenueViewModel>(partial.Model);
        venueService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_WhenModelStateIsInvalid_ReturnsCreateModalPartialAndDoesNotCallService()
    {
        var model = CreateAddVenueViewModel();
        var venueService = new Mock<IVenueService>();
        var controller = CreateController(venueService, Guid.NewGuid());
        controller.ModelState.AddModelError("Name", "Required");

        var result = await controller.Create(model);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_CreateModal", partial.ViewName);
        Assert.Same(model, partial.Model);
        venueService.Verify(s => s.AddVenueAsync(It.IsAny<AddVenueViewModel>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenModelStateIsValid_CallsAddVenueAsyncWithAdminIdAndReturnsJsonSuccess()
    {
        var adminId = Guid.NewGuid();
        var model = CreateAddVenueViewModel();
        var venueService = new Mock<IVenueService>();
        venueService
            .Setup(s => s.AddVenueAsync(model, adminId))
            .Returns(Task.CompletedTask);
        var controller = CreateController(venueService, adminId);

        var result = await controller.Create(model);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        venueService.Verify(s => s.AddVenueAsync(model, adminId), Times.Once);
    }

    [Fact]
    public async Task EditPartial_WhenVenueExists_ReturnsEditModalPartialWithModel()
    {
        var venueId = Guid.NewGuid();
        var model = CreateEditVenueViewModel(venueId);
        var venueService = new Mock<IVenueService>();
        venueService
            .Setup(s => s.GetForEditAsync(venueId))
            .ReturnsAsync(model);
        var controller = new VenuesController(venueService.Object);

        var result = await controller.EditPartial(venueId);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_EditModal", partial.ViewName);
        Assert.Same(model, partial.Model);
        venueService.Verify(s => s.GetForEditAsync(venueId), Times.Once);
    }

    [Fact]
    public async Task EditPartial_WhenVenueDoesNotExist_ReturnsNotFound()
    {
        var venueId = Guid.NewGuid();
        var venueService = new Mock<IVenueService>();
        venueService
            .Setup(s => s.GetForEditAsync(venueId))
            .ReturnsAsync((EditVenueViewModel?)null);
        var controller = new VenuesController(venueService.Object);

        var result = await controller.EditPartial(venueId);

        Assert.IsType<NotFoundResult>(result);
        venueService.Verify(s => s.GetForEditAsync(venueId), Times.Once);
    }

    [Fact]
    public async Task Edit_WhenModelStateIsInvalid_ReturnsEditModalPartialAndDoesNotCallService()
    {
        var model = CreateEditVenueViewModel(Guid.NewGuid());
        var venueService = new Mock<IVenueService>();
        var controller = new VenuesController(venueService.Object);
        controller.ModelState.AddModelError("Name", "Required");

        var result = await controller.Edit(model);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_EditModal", partial.ViewName);
        Assert.Same(model, partial.Model);
        venueService.Verify(s => s.UpdateAsync(It.IsAny<EditVenueViewModel>()), Times.Never);
    }

    [Fact]
    public async Task Edit_WhenServiceReturnsFalse_ReturnsNotFound()
    {
        var model = CreateEditVenueViewModel(Guid.NewGuid());
        var venueService = new Mock<IVenueService>();
        venueService
            .Setup(s => s.UpdateAsync(model))
            .ReturnsAsync(false);
        var controller = new VenuesController(venueService.Object);

        var result = await controller.Edit(model);

        Assert.IsType<NotFoundResult>(result);
        venueService.Verify(s => s.UpdateAsync(model), Times.Once);
    }

    [Fact]
    public async Task Edit_WhenServiceReturnsTrue_ReturnsJsonSuccess()
    {
        var model = CreateEditVenueViewModel(Guid.NewGuid());
        var venueService = new Mock<IVenueService>();
        venueService
            .Setup(s => s.UpdateAsync(model))
            .ReturnsAsync(true);
        var controller = new VenuesController(venueService.Object);

        var result = await controller.Edit(model);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        venueService.Verify(s => s.UpdateAsync(model), Times.Once);
    }

    [Fact]
    public async Task Deactivate_CallsDeactivateAsyncAndReturnsJsonSuccess()
    {
        var venueId = Guid.NewGuid();
        var venueService = new Mock<IVenueService>();
        venueService
            .Setup(s => s.DeactivateAsync(venueId))
            .ReturnsAsync(true);
        var controller = new VenuesController(venueService.Object);

        var result = await controller.Deactivate(venueId);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        venueService.Verify(s => s.DeactivateAsync(venueId), Times.Once);
    }

    private static VenuesController CreateController(Mock<IVenueService> venueService, Guid adminId)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, adminId.ToString()) },
                "TestAuth"))
        };

        return new VenuesController(venueService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static AddVenueViewModel CreateAddVenueViewModel()
        => new()
        {
            Name = "Main Hall",
            Description = "Large venue",
            Address = "1 Main Street",
            City = "Sofia",
            Country = "Bulgaria",
            PostalCode = "1000",
            Latitude = 42.6977f,
            Longitude = 23.3219f,
            ContactEmail = "venue@example.com",
            ContactPhone = "123456"
        };

    private static EditVenueViewModel CreateEditVenueViewModel(Guid id)
        => new()
        {
            Id = id,
            Name = "Main Hall",
            Description = "Large venue",
            Address = "1 Main Street",
            City = "Sofia",
            Country = "Bulgaria",
            PostalCode = "1000",
            Latitude = 42.6977f,
            Longitude = 23.3219f,
            ContactEmail = "venue@example.com",
            ContactPhone = "123456"
        };

    private static bool ReadSuccessProperty(object? value)
    {
        Assert.NotNull(value);
        var property = value.GetType().GetProperty("success");
        Assert.NotNull(property);
        return Assert.IsType<bool>(property.GetValue(value));
    }
}
