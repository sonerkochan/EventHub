using System.Security.Claims;
using EventHub.Areas.Admin.Controllers;
using EventHub.Core.Contracts;
using EventHub.Core.Models.Event;
using EventHub.Core.Models.Room;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;

namespace EventHub.Tests.Unit.Admin;

[Trait("Category", "Unit")]
public class EventsControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithEvents()
    {
        var events = new List<EventListViewModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                EventName = "Concert",
                EventType = EventType.Concert,
                EventStatus = EventStatus.Published,
                EventPriority = EventPriority.Normal,
                BasePrice = 25,
                IsActive = true
            }
        };
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.GetAllEventsAsync())
            .ReturnsAsync(events);
        var controller = CreateController(eventService);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<EventListViewModel>>(viewResult.Model);
        Assert.Same(events, model);
        eventService.Verify(s => s.GetAllEventsAsync(), Times.Once);
    }

    [Fact]
    public async Task CreatePartial_ReturnsCreateModalPartialWithCreateEventViewModelAndRooms()
    {
        var rooms = CreateRooms();
        var roomService = new Mock<IRoomService>();
        roomService
            .Setup(s => s.GetAllRoomsAsync())
            .ReturnsAsync(rooms);
        var controller = CreateController(roomService: roomService);

        var result = await controller.CreatePartial();

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_CreateModal", partial.ViewName);
        var model = Assert.IsType<CreateEventViewModel>(partial.Model);
        Assert.Equal(rooms.Select(r => r.Id.ToString()), model.AvailableRooms.Select(r => r.Value));
        Assert.All(model.AvailableRooms, item => Assert.IsType<SelectListItem>(item));
        roomService.Verify(s => s.GetAllRoomsAsync(), Times.Once);
    }

    [Fact]
    public async Task Create_WhenModelStateIsInvalid_ReturnsCreateModalPartialAndDoesNotCallCreateAsync()
    {
        var model = CreateCreateEventViewModel();
        var eventService = new Mock<IEventService>();
        var roomService = new Mock<IRoomService>();
        roomService
            .Setup(s => s.GetAllRoomsAsync())
            .ReturnsAsync(CreateRooms());
        var controller = CreateController(eventService, roomService);
        controller.ModelState.AddModelError("EventName", "Required");

        var result = await controller.Create(model);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_CreateModal", partial.ViewName);
        Assert.Same(model, partial.Model);
        Assert.NotEmpty(model.AvailableRooms);
        eventService.Verify(s => s.CreateAsync(It.IsAny<CreateEventViewModel>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenModelStateIsValid_CallsCreateAsyncWithAdminIdAndReturnsJsonSuccess()
    {
        var adminId = Guid.NewGuid();
        var model = CreateCreateEventViewModel();
        var eventId = Guid.NewGuid();
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.CreateAsync(model, adminId))
            .ReturnsAsync(eventId);
        var controller = CreateController(eventService, adminId: adminId);

        var result = await controller.Create(model);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        Assert.Equal("https://example.com/cover.jpg", model.CoverImageUrl);
        eventService.Verify(s => s.CreateAsync(model, adminId), Times.Once);
    }

    [Fact]
    public async Task EditPartial_WhenEventExists_ReturnsEditModalPartialWithModelAndRooms()
    {
        var eventId = Guid.NewGuid();
        var model = CreateEditEventViewModel(eventId);
        var rooms = CreateRooms();
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.GetEventForEditAsync(eventId))
            .ReturnsAsync(model);
        var roomService = new Mock<IRoomService>();
        roomService
            .Setup(s => s.GetAllRoomsAsync())
            .ReturnsAsync(rooms);
        var controller = CreateController(eventService, roomService);

        var result = await controller.EditPartial(eventId);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_EditModal", partial.ViewName);
        Assert.Same(model, partial.Model);
        Assert.Equal(rooms.Select(r => r.Id.ToString()), model.AvailableRooms.Select(r => r.Value));
        eventService.Verify(s => s.GetEventForEditAsync(eventId), Times.Once);
        roomService.Verify(s => s.GetAllRoomsAsync(), Times.Once);
    }

    [Fact]
    public async Task EditPartial_WhenEventDoesNotExist_ReturnsNotFound()
    {
        var eventId = Guid.NewGuid();
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.GetEventForEditAsync(eventId))
            .ReturnsAsync((EditEventViewModel?)null);
        var controller = CreateController(eventService);

        var result = await controller.EditPartial(eventId);

        Assert.IsType<NotFoundResult>(result);
        eventService.Verify(s => s.GetEventForEditAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task Edit_WhenModelStateIsInvalid_ReturnsEditModalPartialAndDoesNotCallUpdateAsync()
    {
        var model = CreateEditEventViewModel(Guid.NewGuid());
        var eventService = new Mock<IEventService>();
        var roomService = new Mock<IRoomService>();
        roomService
            .Setup(s => s.GetAllRoomsAsync())
            .ReturnsAsync(CreateRooms());
        var controller = CreateController(eventService, roomService);
        controller.ModelState.AddModelError("EventName", "Required");

        var result = await controller.Edit(model);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_EditModal", partial.ViewName);
        Assert.Same(model, partial.Model);
        Assert.NotEmpty(model.AvailableRooms);
        eventService.Verify(s => s.UpdateAsync(It.IsAny<EditEventViewModel>()), Times.Never);
    }

    [Fact]
    public async Task Edit_WhenServiceReturnsFalse_ReturnsNotFound()
    {
        var model = CreateEditEventViewModel(Guid.NewGuid());
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.UpdateAsync(model))
            .ReturnsAsync(false);
        var controller = CreateController(eventService);

        var result = await controller.Edit(model);

        Assert.IsType<NotFoundResult>(result);
        eventService.Verify(s => s.UpdateAsync(model), Times.Once);
    }

    [Fact]
    public async Task Edit_WhenServiceReturnsTrue_ReturnsJsonSuccess()
    {
        var model = CreateEditEventViewModel(Guid.NewGuid());
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.UpdateAsync(model))
            .ReturnsAsync(true);
        var controller = CreateController(eventService);

        var result = await controller.Edit(model);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        eventService.Verify(s => s.UpdateAsync(model), Times.Once);
    }

    [Fact]
    public async Task DetailsPartial_WhenEventExists_ReturnsDetailsModalPartialWithModel()
    {
        var eventId = Guid.NewGuid();
        var model = CreateEventDetailViewModel(eventId);
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.GetEventByIdAsync(eventId))
            .ReturnsAsync(model);
        var controller = CreateController(eventService);

        var result = await controller.DetailsPartial(eventId);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_DetailsModal", partial.ViewName);
        Assert.Same(model, partial.Model);
        eventService.Verify(s => s.GetEventByIdAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task DetailsPartial_WhenEventDoesNotExist_ReturnsNotFound()
    {
        var eventId = Guid.NewGuid();
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.GetEventByIdAsync(eventId))
            .ReturnsAsync((EventDetailViewModel?)null);
        var controller = CreateController(eventService);

        var result = await controller.DetailsPartial(eventId);

        Assert.IsType<NotFoundResult>(result);
        eventService.Verify(s => s.GetEventByIdAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task Deactivate_CallsDeactivateAsyncAndReturnsJsonSuccess()
    {
        var eventId = Guid.NewGuid();
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.DeactivateAsync(eventId))
            .ReturnsAsync(true);
        var controller = CreateController(eventService);

        var result = await controller.Deactivate(eventId);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        eventService.Verify(s => s.DeactivateAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task Publish_CallsPublishAsyncAndReturnsJsonSuccess()
    {
        var eventId = Guid.NewGuid();
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.PublishAsync(eventId))
            .ReturnsAsync(true);
        var controller = CreateController(eventService);

        var result = await controller.Publish(eventId);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(ReadSuccessProperty(json.Value));
        eventService.Verify(s => s.PublishAsync(eventId), Times.Once);
    }

    private static EventsController CreateController(
        Mock<IEventService>? eventService = null,
        Mock<IRoomService>? roomService = null,
        Mock<IPhotoService>? photoService = null,
        Guid? adminId = null)
    {
        var resolvedAdminId = adminId ?? Guid.NewGuid();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, resolvedAdminId.ToString()) },
                "TestAuth"))
        };

        return new EventsController(
            (eventService ?? new Mock<IEventService>()).Object,
            (roomService ?? new Mock<IRoomService>()).Object,
            (photoService ?? new Mock<IPhotoService>()).Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static List<RoomListViewModel> CreateRooms()
        =>
        [
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
        ];

    private static CreateEventViewModel CreateCreateEventViewModel()
        => new()
        {
            EventName = "Concert",
            Description = "Live music event",
            EventType = EventType.Concert,
            EventPriority = EventPriority.Normal,
            RoomId = Guid.NewGuid(),
            StartDateTime = new DateTime(2026, 6, 1, 18, 0, 0),
            EndDateTime = new DateTime(2026, 6, 1, 20, 0, 0),
            TotalTickets = 100,
            BasePrice = 25,
            AllowRefunds = true,
            RefundDeadline = new DateTime(2026, 5, 31, 18, 0, 0),
            CoverImageUrl = "https://example.com/cover.jpg",
            Address = "1 Main Street",
            City = "Sofia",
            CountryCode = "BG",
            Latitude = 42.6767m,
            Longitude = 23.3219m
        };

    private static EditEventViewModel CreateEditEventViewModel(Guid id)
        => new()
        {
            Id = id,
            EventName = "Concert",
            Description = "Live music event",
            EventType = EventType.Concert,
            EventStatus = EventStatus.Draft,
            EventPriority = EventPriority.Normal,
            RoomId = Guid.NewGuid(),
            StartDateTime = new DateTime(2026, 6, 1, 18, 0, 0),
            EndDateTime = new DateTime(2026, 6, 1, 20, 0, 0),
            TotalTickets = 100,
            BasePrice = 25,
            AllowRefunds = true,
            RefundDeadline = new DateTime(2026, 5, 31, 18, 0, 0),
            CoverImageUrl = "https://example.com/cover.jpg",
            Address = "1 Main Street",
            City = "Sofia",
            CountryCode = "BG",
            Latitude = 42.6767m,
            Longitude = 23.3219m
        };

    private static EventDetailViewModel CreateEventDetailViewModel(Guid id)
        => new()
        {
            Id = id,
            EventName = "Concert",
            Description = "Live music event",
            EventType = EventType.Concert,
            EventStatus = EventStatus.Published,
            EventPriority = EventPriority.Normal,
            StartDateTime = new DateTime(2026, 6, 1, 18, 0, 0),
            EndDateTime = new DateTime(2026, 6, 1, 20, 0, 0),
            TotalTickets = 100,
            TicketsSold = 10,
            BasePrice = 25,
            AllowRefunds = true,
            RefundDeadline = new DateTime(2026, 5, 31, 18, 0, 0),
            IsActive = true,
            RoomId = Guid.NewGuid(),
            RoomName = "Main Room",
            CoverImageUrl = "https://example.com/cover.jpg",
            City = "Sofia",
            CountryCode = "BG",
            Latitude = 42.6767m,
            Longitude = 23.3219m
        };

    private static bool ReadSuccessProperty(object? value)
    {
        Assert.NotNull(value);
        var property = value.GetType().GetProperty("success");
        Assert.NotNull(property);
        return Assert.IsType<bool>(property.GetValue(value));
    }
}
