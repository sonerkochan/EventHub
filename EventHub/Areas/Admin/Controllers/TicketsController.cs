using EventHub.Core.Contracts;
using EventHub.Core.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Admin.Controllers
{
    public class TicketsController : BaseController
    {
        private readonly IEventService eventService;
        private readonly IEventPricingTierService pricingTierService;

        public TicketsController(IEventService _eventService, IEventPricingTierService _pricingTierService)
        {
            eventService = _eventService;
            pricingTierService = _pricingTierService;
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
    }
}
