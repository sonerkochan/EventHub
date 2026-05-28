using System.Security.Claims;
using EventHub.Areas.Admin.Controllers;
using EventHub.Core.Contracts;
using EventHub.Core.Models.Room;
using EventHub.Core.Models.Venue;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;

namespace EventHub.Tests.Unit.Admin;

public class RoomsControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithRooms()
    {
        var rooms = new List<RoomListViewModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                VenueId = Guid.NewGuid(),
                VenueName = "Main Venue",
                Name = "Main Room",
                Capacity = 100,
                RoomType = RoomType.Auditorium,
                IsActive = true
            }
        };
        var roomService = new Mock<IRoomService>();
        roomService
            .Setup(s => s.GetAllRoomsAsync())
            .ReturnsAsync(rooms);
        var controller = CreateController(roomService);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<RoomListViewModel>>(viewResult.Model);
        Assert.Same(rooms, model);
        roomService.Verify(s => s.GetAllRoomsAsync(), Times.Once);
    }

    [Fact]
    public async Task CreatePartial_ReturnsCreateModalPartialWithAddRoomViewModelAndVenues()
    {
        var venues = CreateVenues();
        var venueService = new Mock<IVenueService>();
        venueService
            .Setup(s => s.GetAllVenuesAsync())
            .ReturnsAsync(venues);

        var controller = CreateController(venueService: venueService);

        var result = await controller.CreatePartial();

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_CreateModal", partial.ViewName);
        Assert.IsType<AddRoomViewModel>(partial.Model);

        object venuesObject = controller.ViewBag.Venues;

        var selectList = Assert
            .IsAssignableFrom<IEnumerable<SelectListItem>>(venuesObject)
            .ToList();

        var expectedValues = venues
            .Select(v => v.Id.ToString())
            .ToList();

        var actualValues = selectList
            .Select(v => v.Value)
            .ToList();

        Assert.Equal(expectedValues, actualValues);

        venueService.Verify(s => s.GetAllVenuesAsync(), Times.Once);
    }

    [Fact]
    public async Task Create_WhenModelStateIsInvalid_ReturnsCreateModalPartialAndDoesNotCallRoomService()
    {
        var model = CreateAddRoomViewModel();
        var roomService = new Mock<IRoomService>();
        var venueService = new Mock<IVenueService>();
        venueService
            .Setup(s => s.GetAllVenuesAsync())
            .ReturnsAsync(CreateVenues());
        var controller = CreateController(roomService, venueService);
        controller.ModelState.AddModelError("Name", "Required");

        var result = await controller.Create(model);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_CreateModal", partial.ViewName);
        Assert.Same(model, partial.Model);
        Assert.IsAssignableFrom<IEnumerable<SelectListItem>>(controller.ViewBag.Venues);
        roomService.Verify(s => s.AddRoomAsync(It.IsAny<AddRoomViewModel>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenModelStateIsValid_CallsAddRoomAsyncWithAdminIdAndReturnsJsonSuccess()
    {
        var adminId = Guid.NewGuid();
        var model = CreateAddRoomViewModel();
        var roomService = new Mock<IRoomService>();
        roomService
            .Setup(s => s.AddRoomAsync(model, adminId))
            .ReturnsAsync(Guid.NewGuid());
        var controller = CreateController(roomService, adminId: adminId);

        var result = await controller.Create(model);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        roomService.Verify(s => s.AddRoomAsync(model, adminId), Times.Once);
    }

    [Fact]
    public async Task EditPartial_WhenRoomExists_ReturnsEditModalPartialWithModelAndVenues()
    {
        var roomId = Guid.NewGuid();
        var model = CreateEditRoomViewModel(roomId);
        var roomService = new Mock<IRoomService>();
        roomService
            .Setup(s => s.GetRoomForEditAsync(roomId))
            .ReturnsAsync(model);
        var venueService = new Mock<IVenueService>();
        venueService
            .Setup(s => s.GetAllVenuesAsync())
            .ReturnsAsync(CreateVenues());
        var controller = CreateController(roomService, venueService);

        var result = await controller.EditPartial(roomId);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_EditModal", partial.ViewName);
        Assert.Same(model, partial.Model);
        Assert.IsAssignableFrom<IEnumerable<SelectListItem>>(controller.ViewBag.Venues);
        roomService.Verify(s => s.GetRoomForEditAsync(roomId), Times.Once);
        venueService.Verify(s => s.GetAllVenuesAsync(), Times.Once);
    }

    [Fact]
    public async Task EditPartial_WhenRoomDoesNotExist_ReturnsNotFound()
    {
        var roomId = Guid.NewGuid();
        var roomService = new Mock<IRoomService>();
        roomService
            .Setup(s => s.GetRoomForEditAsync(roomId))
            .ReturnsAsync((EditRoomViewModel?)null);
        var controller = CreateController(roomService);

        var result = await controller.EditPartial(roomId);

        Assert.IsType<NotFoundResult>(result);
        roomService.Verify(s => s.GetRoomForEditAsync(roomId), Times.Once);
    }

    [Fact]
    public async Task Edit_WhenModelStateIsInvalid_ReturnsEditModalPartialAndDoesNotCallUpdate()
    {
        var model = CreateEditRoomViewModel(Guid.NewGuid());
        var roomService = new Mock<IRoomService>();
        var venueService = new Mock<IVenueService>();
        venueService
            .Setup(s => s.GetAllVenuesAsync())
            .ReturnsAsync(CreateVenues());
        var controller = CreateController(roomService, venueService);
        controller.ModelState.AddModelError("Name", "Required");

        var result = await controller.Edit(model);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_EditModal", partial.ViewName);
        Assert.Same(model, partial.Model);
        Assert.IsAssignableFrom<IEnumerable<SelectListItem>>(controller.ViewBag.Venues);
        roomService.Verify(s => s.UpdateRoomAsync(It.IsAny<EditRoomViewModel>()), Times.Never);
    }

    [Fact]
    public async Task Edit_WhenServiceReturnsFalse_ReturnsNotFound()
    {
        var model = CreateEditRoomViewModel(Guid.NewGuid());
        var roomService = new Mock<IRoomService>();
        roomService
            .Setup(s => s.UpdateRoomAsync(model))
            .ReturnsAsync(false);
        var controller = CreateController(roomService);

        var result = await controller.Edit(model);

        Assert.IsType<NotFoundResult>(result);
        roomService.Verify(s => s.UpdateRoomAsync(model), Times.Once);
    }

    [Fact]
    public async Task Edit_WhenServiceReturnsTrue_ReturnsJsonSuccess()
    {
        var model = CreateEditRoomViewModel(Guid.NewGuid());
        var roomService = new Mock<IRoomService>();
        roomService
            .Setup(s => s.UpdateRoomAsync(model))
            .ReturnsAsync(true);
        var controller = CreateController(roomService);

        var result = await controller.Edit(model);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        roomService.Verify(s => s.UpdateRoomAsync(model), Times.Once);
    }

    [Fact]
    public async Task Deactivate_CallsDeactivateRoomAsyncAndReturnsJsonSuccess()
    {
        var roomId = Guid.NewGuid();
        var roomService = new Mock<IRoomService>();
        roomService
            .Setup(s => s.DeactivateRoomAsync(roomId))
            .ReturnsAsync(true);
        var controller = CreateController(roomService);

        var result = await controller.Deactivate(roomId);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        roomService.Verify(s => s.DeactivateRoomAsync(roomId), Times.Once);
    }

    [Fact]
    public async Task Layout_ReturnsViewWithSeatLayoutEditorModel()
    {
        var roomId = Guid.NewGuid();
        var model = CreateLayoutModel(roomId);
        var seatLayoutService = new Mock<ISeatLayoutService>();
        seatLayoutService
            .Setup(s => s.GetLayoutEditorDataAsync(roomId))
            .ReturnsAsync(model);
        var controller = CreateController(seatLayoutService: seatLayoutService);

        var result = await controller.Layout(roomId);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        seatLayoutService.Verify(s => s.GetLayoutEditorDataAsync(roomId), Times.Once);
    }

    [Fact]
    public async Task SaveLayout_WhenServiceSucceeds_ReturnsJsonSuccessWithSeatsAndZones()
    {
        var adminId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var request = new SaveSeatLayoutRequest { RoomId = roomId, GridRows = 2, GridColumns = 2 };
        var layout = CreateLayoutModel(roomId);
        var seatLayoutService = new Mock<ISeatLayoutService>();
        seatLayoutService
            .Setup(s => s.SaveLayoutAsync(request, adminId))
            .Returns(Task.CompletedTask);
        seatLayoutService
            .Setup(s => s.GetLayoutEditorDataAsync(roomId))
            .ReturnsAsync(layout);
        var controller = CreateController(seatLayoutService: seatLayoutService, adminId: adminId);

        var result = await controller.SaveLayout(request);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        Assert.Same(layout.Seats, ReadProperty<List<SeatDto>>(json.Value, "seats"));
        Assert.Same(layout.Zones, ReadProperty<List<ZoneDto>>(json.Value, "zones"));
        seatLayoutService.Verify(s => s.SaveLayoutAsync(request, adminId), Times.Once);
        seatLayoutService.Verify(s => s.GetLayoutEditorDataAsync(roomId), Times.Once);
    }

    [Fact]
    public async Task SaveLayout_WhenServiceThrowsInvalidOperationException_ReturnsJsonFailure()
    {
        var adminId = Guid.NewGuid();
        var request = new SaveSeatLayoutRequest { RoomId = Guid.NewGuid(), GridRows = 2, GridColumns = 2 };
        var seatLayoutService = new Mock<ISeatLayoutService>();
        seatLayoutService
            .Setup(s => s.SaveLayoutAsync(request, adminId))
            .ThrowsAsync(new InvalidOperationException("Seat count exceeds capacity."));
        var controller = CreateController(seatLayoutService: seatLayoutService, adminId: adminId);

        var result = await controller.SaveLayout(request);

        var json = Assert.IsType<JsonResult>(result);
        Assert.False(ReadSuccessProperty(json.Value));
        Assert.Equal("Seat count exceeds capacity.", ReadProperty<string>(json.Value, "message"));
        seatLayoutService.Verify(s => s.GetLayoutEditorDataAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CreateZone_CallsCreateZoneAsyncAndReturnsJsonSuccess()
    {
        var adminId = Guid.NewGuid();
        var request = new CreateZoneRequest
        {
            RoomId = Guid.NewGuid(),
            Name = "VIP",
            ZoneType = ZoneType.VIP
        };
        var zone = new ZoneDto { Id = Guid.NewGuid(), Name = "VIP", ZoneType = ZoneType.VIP };
        var seatLayoutService = new Mock<ISeatLayoutService>();
        seatLayoutService
            .Setup(s => s.CreateZoneAsync(request, adminId))
            .ReturnsAsync(zone);
        var controller = CreateController(seatLayoutService: seatLayoutService, adminId: adminId);

        var result = await controller.CreateZone(request);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        Assert.Same(zone, ReadProperty<ZoneDto>(json.Value, "zone"));
        seatLayoutService.Verify(s => s.CreateZoneAsync(request, adminId), Times.Once);
    }

    [Fact]
    public async Task AssignZone_CallsServiceAndReturnsUpdatedLayoutData()
    {
        var roomId = Guid.NewGuid();
        var request = new AssignZoneRequest
        {
            RoomId = roomId,
            ZoneId = Guid.NewGuid(),
            SeatIds = [Guid.NewGuid()]
        };
        var layout = CreateLayoutModel(roomId);
        var seatLayoutService = new Mock<ISeatLayoutService>();
        seatLayoutService
            .Setup(s => s.AssignSeatsToZoneAsync(request))
            .Returns(Task.CompletedTask);
        seatLayoutService
            .Setup(s => s.GetLayoutEditorDataAsync(roomId))
            .ReturnsAsync(layout);
        var controller = CreateController(seatLayoutService: seatLayoutService);

        var result = await controller.AssignZone(request);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        Assert.Same(layout.Seats, ReadProperty<List<SeatDto>>(json.Value, "seats"));
        Assert.Same(layout.Zones, ReadProperty<List<ZoneDto>>(json.Value, "zones"));
        seatLayoutService.Verify(s => s.AssignSeatsToZoneAsync(request), Times.Once);
        seatLayoutService.Verify(s => s.GetLayoutEditorDataAsync(roomId), Times.Once);
    }

    [Fact]
    public async Task RemoveFromZone_CallsServiceAndReturnsUpdatedLayoutData()
    {
        var roomId = Guid.NewGuid();
        var request = new RemoveFromZoneRequest
        {
            RoomId = roomId,
            SeatIds = [Guid.NewGuid()]
        };
        var layout = CreateLayoutModel(roomId);
        var seatLayoutService = new Mock<ISeatLayoutService>();
        seatLayoutService
            .Setup(s => s.RemoveSeatsFromZoneAsync(request))
            .Returns(Task.CompletedTask);
        seatLayoutService
            .Setup(s => s.GetLayoutEditorDataAsync(roomId))
            .ReturnsAsync(layout);
        var controller = CreateController(seatLayoutService: seatLayoutService);

        var result = await controller.RemoveFromZone(request);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        Assert.Same(layout.Seats, ReadProperty<List<SeatDto>>(json.Value, "seats"));
        Assert.Same(layout.Zones, ReadProperty<List<ZoneDto>>(json.Value, "zones"));
        seatLayoutService.Verify(s => s.RemoveSeatsFromZoneAsync(request), Times.Once);
        seatLayoutService.Verify(s => s.GetLayoutEditorDataAsync(roomId), Times.Once);
    }

    [Fact]
    public async Task DeleteZone_CallsDeleteZoneAsyncAndReturnsJsonSuccess()
    {
        var request = new DeleteZoneRequest { Id = Guid.NewGuid() };
        var seatLayoutService = new Mock<ISeatLayoutService>();
        seatLayoutService
            .Setup(s => s.DeleteZoneAsync(request.Id))
            .Returns(Task.CompletedTask);
        var controller = CreateController(seatLayoutService: seatLayoutService);

        var result = await controller.DeleteZone(request);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        seatLayoutService.Verify(s => s.DeleteZoneAsync(request.Id), Times.Once);
    }

    private static RoomsController CreateController(
        Mock<IRoomService>? roomService = null,
        Mock<IVenueService>? venueService = null,
        Mock<ISeatLayoutService>? seatLayoutService = null,
        Guid? adminId = null)
    {
        var resolvedAdminId = adminId ?? Guid.NewGuid();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, resolvedAdminId.ToString()) },
                "TestAuth"))
        };

        return new RoomsController(
            (roomService ?? new Mock<IRoomService>()).Object,
            (venueService ?? new Mock<IVenueService>()).Object,
            (seatLayoutService ?? new Mock<ISeatLayoutService>()).Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static List<VenueListViewModel> CreateVenues()
        =>
        [
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Main Venue",
                City = "Sofia",
                Country = "Bulgaria",
                Address = "1 Main Street"
            }
        ];

    private static AddRoomViewModel CreateAddRoomViewModel()
        => new()
        {
            VenueId = Guid.NewGuid(),
            Name = "Main Room",
            Description = "Room description",
            Capacity = 100,
            RoomType = RoomType.Auditorium,
            IsActive = true
        };

    private static EditRoomViewModel CreateEditRoomViewModel(Guid id)
        => new()
        {
            Id = id,
            VenueId = Guid.NewGuid(),
            Name = "Main Room",
            Description = "Room description",
            Capacity = 100,
            RoomType = RoomType.Auditorium
        };

    private static SeatLayoutEditorViewModel CreateLayoutModel(Guid roomId)
        => new()
        {
            RoomId = roomId,
            RoomName = "Main Room",
            RoomCapacity = 100,
            GridRows = 2,
            GridColumns = 2,
            Seats =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Row = 0,
                    Column = 0,
                    SeatNumber = 1,
                    IsActive = true
                }
            ],
            Zones =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "VIP",
                    ZoneType = ZoneType.VIP,
                    SeatCount = 1
                }
            ]
        };

    private static bool ReadSuccessProperty(object? value)
        => ReadProperty<bool>(value, "success");

    private static T ReadProperty<T>(object? value, string propertyName)
    {
        Assert.NotNull(value);
        var property = value.GetType().GetProperty(propertyName);
        Assert.NotNull(property);
        return Assert.IsType<T>(property.GetValue(value));
    }
}
