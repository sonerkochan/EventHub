using EventHub.Areas.Client.Controllers;
using EventHub.Core.Contracts;
using EventHub.Core.Models.Event;
using EventHub.Core.Models.Travelis;
using EventHub.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;

namespace EventHub.Tests.Unit.Client;

[Trait("Category", "Unit")]
public class ClientEventsControllerTests
{
    [Fact]
    public async Task TravelisHotels_WhenEventExists_CallsServiceWithEventCity()
    {
        var eventId = Guid.NewGuid();
        var hotel = new TravelisHotelViewModel { Id = "hotel-1", Name = "Grand Hotel Sofia" };
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.GetPublishedEventByIdAsync(eventId))
            .ReturnsAsync(new EventDetailViewModel
            {
                Id = eventId,
                EventName = "Concert",
                City = "Sofia"
            });
        var travelisService = new Mock<ITravelisHotelService>();
        travelisService
            .Setup(s => s.GetHotelsByCityAsync("Sofia", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { hotel });
        var controller = CreateController(eventService, travelisService);

        var result = await controller.TravelisHotels(eventId, CancellationToken.None);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_TravelisHotels", partial.ViewName);
        var model = Assert.IsType<TravelisHotelsSectionViewModel>(partial.Model);
        Assert.Equal("Sofia", model.City);
        Assert.Same(hotel, Assert.Single(model.Hotels));
        Assert.False(model.IsUnavailable);
        travelisService.Verify(s => s.GetHotelsByCityAsync("Sofia", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TravelisHotels_WhenEventDoesNotExist_ReturnsNotFound()
    {
        var eventId = Guid.NewGuid();
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.GetPublishedEventByIdAsync(eventId))
            .ReturnsAsync((EventDetailViewModel?)null);
        var travelisService = new Mock<ITravelisHotelService>();
        var controller = CreateController(eventService, travelisService);

        var result = await controller.TravelisHotels(eventId, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
        travelisService.Verify(
            s => s.GetHotelsByCityAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TravelisHotels_WhenEventHasNoCity_ReturnsEmptyPartialWithoutCallingService()
    {
        var eventId = Guid.NewGuid();
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.GetPublishedEventByIdAsync(eventId))
            .ReturnsAsync(new EventDetailViewModel
            {
                Id = eventId,
                EventName = "Concert",
                City = " "
            });
        var travelisService = new Mock<ITravelisHotelService>();
        var controller = CreateController(eventService, travelisService);

        var result = await controller.TravelisHotels(eventId, CancellationToken.None);

        var partial = Assert.IsType<PartialViewResult>(result);
        var model = Assert.IsType<TravelisHotelsSectionViewModel>(partial.Model);
        Assert.True(model.IsMissingCity);
        Assert.Empty(model.Hotels);
        travelisService.Verify(
            s => s.GetHotelsByCityAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TravelisHotels_WhenServiceThrows_RendersUnavailablePartial()
    {
        var eventId = Guid.NewGuid();
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.GetPublishedEventByIdAsync(eventId))
            .ReturnsAsync(new EventDetailViewModel
            {
                Id = eventId,
                EventName = "Concert",
                City = "Sofia"
            });
        var travelisService = new Mock<ITravelisHotelService>();
        travelisService
            .Setup(s => s.GetHotelsByCityAsync("Sofia", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Travelis unavailable."));
        var controller = CreateController(eventService, travelisService);

        var result = await controller.TravelisHotels(eventId, CancellationToken.None);

        var partial = Assert.IsType<PartialViewResult>(result);
        var model = Assert.IsType<TravelisHotelsSectionViewModel>(partial.Model);
        Assert.True(model.IsUnavailable);
        Assert.Empty(model.Hotels);
    }

    private static EventsController CreateController(
        Mock<IEventService> eventService,
        Mock<ITravelisHotelService> travelisHotelService)
    {
        return new EventsController(
            eventService.Object,
            Mock.Of<ITicketService>(),
            Mock.Of<ISeatService>(),
            Mock.Of<IZoneService>(),
            Mock.Of<IEventPricingTierService>(),
            travelisHotelService.Object,
            Options.Create(new TravelisOptions()),
            Mock.Of<IStringLocalizer<MessagesResource>>());
    }
}
