using EventHub.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventsService;

        private IConfiguration conf;

        public EventsController(IEventService eventService, IConfiguration _conf)
        {
            _eventsService = eventService;
            conf = _conf;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _eventsService.GetAllEventsAsync();
            return Ok(events);
        }
    }
}