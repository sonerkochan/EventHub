using EventHub.Core.Contracts;
using EventHub.Core.Models.Travelis;
using EventHub.Core.Models.Ticket;
using EventHub.Localization;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventHub.Areas.Client.Controllers
{
    public class EventsController : BaseController
    {
        private readonly IEventService eventService;
        private readonly ITicketService ticketService;
        private readonly ISeatService seatService;
        private readonly IZoneService zoneService;
        private readonly IEventPricingTierService pricingTierService;
        private readonly ITravelisHotelService travelisHotelService;
        private readonly TravelisOptions travelisOptions;
        private readonly IStringLocalizer<MessagesResource> messagesLocalizer;

        public EventsController(
            IEventService _eventService,
            ITicketService _ticketService,
            ISeatService _seatService,
            IZoneService _zoneService,
            IEventPricingTierService _pricingTierService,
            ITravelisHotelService _travelisHotelService,
            IOptions<TravelisOptions> _travelisOptions,
            IStringLocalizer<MessagesResource> _messagesLocalizer)
        {
            eventService = _eventService;
            ticketService = _ticketService;
            seatService = _seatService;
            zoneService = _zoneService;
            pricingTierService = _pricingTierService;
            travelisHotelService = _travelisHotelService;
            travelisOptions = _travelisOptions.Value;
            messagesLocalizer = _messagesLocalizer;
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
        public async Task<IActionResult> TravelisHotels(Guid eventId, CancellationToken cancellationToken)
        {
            var ev = await eventService.GetPublishedEventByIdAsync(eventId);
            if (ev == null) return NotFound();

            var model = new TravelisHotelsSectionViewModel
            {
                City = ev.City,
                PartnerBaseUrl = travelisOptions.PartnerBaseUrl
            };

            if (string.IsNullOrWhiteSpace(ev.City))
            {
                return PartialView("_TravelisHotels", model);
            }

            try
            {
                model.Hotels = await travelisHotelService.GetHotelsByCityAsync(ev.City, cancellationToken);
            }
            catch
            {
                model.IsUnavailable = true;
            }

            return PartialView("_TravelisHotels", model);
        }

        [HttpGet]
        public async Task<IActionResult> Buy(Guid id)
        {
            var ev = await eventService.GetPublishedEventByIdAsync(id);
            if (ev == null) return NotFound();

            var basePrice = (float)ev.BasePrice;
            var displayPrice = (float)(ev.PriceAmount ?? 0m);
            var model = new PurchaseTicketViewModel
            {
                EventId = ev.Id,
                EventName = ev.EventName,
                EventStart = ev.StartDateTime,
                RoomName = ev.RoomName ?? string.Empty,
                Price = displayPrice,
                BasePrice = basePrice,
                AvailableTickets = ev.TotalTickets - ev.TicketsSold,
                RoomId = ev.RoomId,
                Currency = "EUR"
            };

            var seats = (await seatService.GetByRoomAsync(ev.RoomId))
                .Where(s => s.IsActive)
                .ToList();

            if (seats.Count == 0)
            {
                return View(model);
            }

            var zones = (await zoneService.GetByRoomAsync(ev.RoomId))
                .Where(z => z.IsActive)
                .ToList();
            var zoneById = zones.ToDictionary(z => z.Id);

            var tiers = (await pricingTierService.GetByEventAsync(id)).ToList();
            var tierByZone = tiers.ToDictionary(t => t.ZoneId);

            var allTickets = await ticketService.GetByEventForAdminAsync(id);
            var takenSeatIds = allTickets
                .Where(t => t.Status == TicketStatus.Reserved
                            || t.Status == TicketStatus.Purchased
                            || t.Status == TicketStatus.Used)
                .Where(t => t.SeatId != Guid.Empty)
                .Select(t => t.SeatId)
                .ToHashSet();

            model.GridRows = seats.Max(s => s.Row) + 1;
            model.GridColumns = seats.Max(s => s.Column) + 1;

            model.Seats = seats.Select(s =>
            {
                EventHub.Core.Models.EventPricingTier.PricingTierListViewModel? tier = null;
                if (s.ZoneId.HasValue) tierByZone.TryGetValue(s.ZoneId.Value, out tier);
                EventHub.Core.Models.Zone.ZoneListViewModel? zone = null;
                if (s.ZoneId.HasValue) zoneById.TryGetValue(s.ZoneId.Value, out zone);

                return new ClientSeatDto
                {
                    Id = s.Id,
                    Row = s.Row,
                    Column = s.Column,
                    SeatNumber = s.SeatNumber,
                    ZoneId = s.ZoneId,
                    ZoneName = zone?.Name,
                    ZoneType = zone?.ZoneType,
                    Price = tier?.Price ?? basePrice,
                    Currency = tier?.Currency ?? "EUR",
                    UsesBasePrice = tier == null,
                    IsTaken = takenSeatIds.Contains(s.Id)
                };
            }).ToList();

            model.Zones = zones
                .OrderBy(z => z.DisplayOrder).ThenBy(z => z.Name)
                .Select(z =>
                {
                    tierByZone.TryGetValue(z.Id, out var tier);
                    var seatCount = seats.Count(s => s.ZoneId == z.Id);
                    var soldCount = model.Seats.Count(s => s.ZoneId == z.Id && s.IsTaken);
                    return new ClientZoneDto
                    {
                        Id = z.Id,
                        Name = z.Name ?? "Unnamed Zone",
                        ZoneType = z.ZoneType,
                        SeatCount = seatCount,
                        AvailableCount = Math.Max(0, seatCount - soldCount),
                        Price = tier?.Price ?? basePrice,
                        Currency = tier?.Currency ?? "EUR",
                        UsesBasePrice = tier == null
                    };
                })
                .ToList();

            model.Currency = tiers.FirstOrDefault()?.Currency ?? "EUR";

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
                TempData["Error"] = messagesLocalizer["Messages.Ticket.NotEnoughTickets"].Value;
                return RedirectToAction(nameof(Buy), new { id = eventId });
            }

            TempData["Success"] = messagesLocalizer["Messages.Ticket.Purchased", quantity].Value;
            return RedirectToAction("Index", "Tickets");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReserveSeats(Guid eventId, List<Guid> seatIds)
        {
            if (seatIds == null || seatIds.Count == 0)
            {
                TempData["Error"] = messagesLocalizer["Messages.Ticket.PickSeat"].Value;
                return RedirectToAction(nameof(Buy), new { id = eventId });
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await ticketService.ReserveSeatsAsync(eventId, userId, seatIds);

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage ?? messagesLocalizer["Messages.Ticket.ReserveFailed"].Value;
                return RedirectToAction(nameof(Buy), new { id = eventId });
            }

            TempData["Success"] = messagesLocalizer["Messages.Ticket.SeatsReserved", result.TicketIds.Count].Value;
            return RedirectToAction("Index", "Tickets");
        }
    }
}
