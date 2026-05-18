using EventHub.Core.Contracts;
using EventHub.Core.Models.Event;
using EventHub.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventsService;

        public EventsController(IEventService eventService)
        {
            _eventsService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _eventsService.GetPublishedEventsAsync();

            return Ok(events.Select(MapToApiResponse));
        }

        [HttpGet("city")]
        public async Task<IActionResult> GetEventsByCity([FromQuery] string? city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest(new { error = "The city query parameter is required." });
            }

            var events = await _eventsService.GetPublishedEventsByCityAsync(city);

            return Ok(events.Select(MapToApiResponse));
        }

        private static EventApiResponse MapToApiResponse(EventListViewModel e)
        {
            return new EventApiResponse
            {
                Id = e.Id,
                Name = e.EventName,
                Type = e.EventType.ToString(),
                Status = e.EventStatus.ToString(),
                Description = e.Description,
                TicketPrice = e.BasePrice,
                AvailableTickets = Math.Max(0, e.TotalTickets - e.TicketsSold),
                TotalTickets = e.TotalTickets,
                TicketsSold = e.TicketsSold,
                StartDateTime = e.StartDateTime,
                EndDateTime = e.EndDateTime,
                CoverImageUrl = e.CoverImageUrl,
                RoomName = e.RoomName,
                Location = new EventApiLocationResponse
                {
                    Lat = e.Latitude,
                    Lng = e.Longitude,
                    City = e.City,
                    CountryCode = e.CountryCode,
                    Address = e.Address
                }
            };
        }
    }
}
