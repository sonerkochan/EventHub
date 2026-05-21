using EventHub.Core.Contracts;
using EventHub.Core.Models.Event;
using EventHub.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventsService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IEventService eventService, IMemoryCache cache, ILogger<EventsController> logger    )
        {
            _eventsService = eventService;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            const string cacheKey = "events_all";
            bool cacheHit = _cache.TryGetValue(cacheKey, out IEnumerable<EventListViewModel>? events);

            if (!cacheHit)
            {
                _logger.LogInformation("CACHE MISS — fetching from DB for key: {Key}", cacheKey);
                events = await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    return await _eventsService.GetPublishedEventsAsync();
                });
            }
            else
            {
                _logger.LogInformation("CACHE HIT — returning cached data for key: {Key}", cacheKey);
            }

            return Ok(events!.Select(MapToApiResponse));
        }


        [HttpGet("city")]
        public async Task<IActionResult> GetEventsByCity([FromQuery] string? city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return BadRequest(new { error = "The city query parameter is required." });

            var cacheKey = $"events_city_{city.ToLowerInvariant()}";
            bool cacheHit = _cache.TryGetValue(cacheKey, out IEnumerable<EventListViewModel>? events);
            if (!cacheHit)
            {
                _logger.LogInformation("CACHE MISS - fetching from DB for key: {Key}", cacheKey);
                events = await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    return await _eventsService.GetPublishedEventsByCityAsync(city);
                });
            }
            else
            {
                _logger.LogInformation("CACHE HIT - returning cached data for key: {Key}", cacheKey);
            }

            return Ok(events!.Select(MapToApiResponse));
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
                CoverImageUrl = e.CoverImageDisplayUrl,
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
