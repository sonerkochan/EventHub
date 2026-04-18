using EventHub.Core.Contracts;
using EventHub.Core.Models.Ticket;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventHub.Areas.Client.Controllers
{
    public class EventsController : BaseController
    {
        private readonly IEventService eventService;
        private readonly ITicketService ticketService;

        public EventsController(IEventService _eventService, ITicketService _ticketService)
        {
            eventService = _eventService;
            ticketService = _ticketService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await eventService.GetPublishedEventsAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var model = await eventService.GetPublishedEventByIdAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Buy(Guid id)
        {
            var ev = await eventService.GetPublishedEventByIdAsync(id);
            if (ev == null) return NotFound();

            var model = new PurchaseTicketViewModel
            {
                EventId = ev.Id,
                EventName = ev.EventName,
                EventStart = ev.StartDateTime,
                RoomName = ev.RoomName ?? string.Empty,
                Price = (float)ev.BasePrice,
                AvailableTickets = ev.TotalTickets - ev.TicketsSold
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyDirect(Guid eventId, int quantity)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ticketIds = await ticketService.PurchaseAsync(eventId, userId, quantity);

            if (ticketIds.Count == 0)
            {
                TempData["Error"] = "Unable to complete purchase. Not enough tickets available.";
                return RedirectToAction(nameof(Buy), new { id = eventId });
            }

            TempData["Success"] = $"{quantity} ticket(s) purchased successfully!";
            return RedirectToAction("Index", "Tickets");
        }
    }
}