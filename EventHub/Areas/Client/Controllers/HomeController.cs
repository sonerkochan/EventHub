using EventHub.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.Areas.Client.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IEventService eventService;
        private readonly ITicketService ticketService;

        public HomeController(IEventService _eventService, ITicketService _ticketService)
        {
            eventService = _eventService;
            ticketService = _ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            ViewBag.UpcomingEvents = await eventService.GetPublishedEventsAsync();
            ViewBag.MyTickets = await ticketService.GetUserTicketsAsync(userId);

            return View();
        }
    }
}
