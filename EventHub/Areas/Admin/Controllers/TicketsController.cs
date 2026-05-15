using EventHub.Core.Contracts;
using EventHub.Core.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.Areas.Admin.Controllers
{
    public class TicketsController : BaseController
    {
        private readonly IEventService eventService;
        private readonly IEventPricingTierService pricingTierService;
        private readonly ITicketService ticketService;

        public TicketsController(
            IEventService _eventService,
            IEventPricingTierService _pricingTierService,
            ITicketService _ticketService)
        {
            eventService = _eventService;
            pricingTierService = _pricingTierService;
            ticketService = _ticketService;
        }

        public async Task<IActionResult> Index()
        {
            var events = await eventService.GetAllEventsAsync();

            var rows = new List<TicketsOverviewRow>();
            foreach (var e in events)
            {
                var tiers = await pricingTierService.GetByEventAsync(e.Id);
                rows.Add(new TicketsOverviewRow
                {
                    EventId = e.Id,
                    EventName = e.EventName,
                    RoomName = e.RoomName,
                    StartDateTime = e.StartDateTime,
                    EventStatus = e.EventStatus,
                    TicketsSold = e.TicketsSold,
                    TotalTickets = e.TotalTickets,
                    HasPricing = tiers.Any()
                });
            }

            return View(rows.OrderByDescending(r => r.StartDateTime).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> LookupPartial(string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return PartialView("_LookupModal", (AdminTicketLookupDto?)null);
            }

            var dto = await ticketService.LookupAsync(q);
            ViewBag.SearchedQuery = q.Trim();
            return PartialView("_LookupModal", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(Guid ticketId)
        {
            var processedBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ok = await ticketService.AdminRefundTicketAsync(ticketId, processedBy);
            return Json(new { success = ok });
        }
    }
}
