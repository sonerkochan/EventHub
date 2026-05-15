using EventHub.Core.Contracts;
using EventHub.Core.Models.Admin;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.Areas.Admin.Controllers
{
    public class EventTicketsController : BaseController
    {
        private readonly IEventService eventService;
        private readonly IZoneService zoneService;
        private readonly IEventPricingTierService pricingTierService;
        private readonly ISeatService seatService;
        private readonly ITicketService ticketService;

        public EventTicketsController(
            IEventService _eventService,
            IZoneService _zoneService,
            IEventPricingTierService _pricingTierService,
            ISeatService _seatService,
            ITicketService _ticketService)
        {
            eventService = _eventService;
            zoneService = _zoneService;
            pricingTierService = _pricingTierService;
            seatService = _seatService;
            ticketService = _ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> Pricing(Guid id)
        {
            var ev = await eventService.GetEventByIdAsync(id);
            if (ev == null) return NotFound();

            var zones = await zoneService.GetByRoomAsync(ev.RoomId);
            var tiers = await pricingTierService.GetByEventAsync(id);
            var tiersByZone = tiers.ToDictionary(t => t.ZoneId);

            var seats = await seatService.GetByRoomAsync(ev.RoomId);
            var liveSeatCountByZone = seats
                .Where(s => s.IsActive && s.ZoneId.HasValue)
                .GroupBy(s => s.ZoneId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            var rows = zones
                .Where(z => z.IsActive)
                .OrderBy(z => z.DisplayOrder)
                .ThenBy(z => z.Name)
                .Select(z =>
                {
                    var hasTier = tiersByZone.TryGetValue(z.Id, out var tier);
                    var liveSeatCount = liveSeatCountByZone.TryGetValue(z.Id, out var cnt) ? cnt : 0;
                    return new ZonePricingRow
                    {
                        ZoneId = z.Id,
                        ZoneName = z.Name ?? "Unnamed Zone",
                        ZoneType = z.ZoneType,
                        SeatCount = liveSeatCount,
                        TierId = hasTier ? tier!.Id : null,
                        Price = hasTier ? tier!.Price : null,
                        Currency = hasTier ? tier!.Currency : null,
                        AvailableQuantity = hasTier ? tier!.AvailableQuantity : null,
                        SoldQuantity = hasTier ? tier!.SoldQuantity : 0
                    };
                })
                .ToList();

            var model = new EventPricingPageViewModel
            {
                EventId = ev.Id,
                EventName = ev.EventName,
                RoomId = ev.RoomId,
                RoomName = ev.RoomName,
                Zones = rows
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SetPrice([FromBody] SetZonePriceRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid price input." });
            }

            try
            {
                var tierId = await pricingTierService.SetForZoneAsync(request);
                return Json(new { success = true, tierId });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemovePrice([FromBody] RemoveZonePriceRequest request)
        {
            var removed = await pricingTierService.RemoveForZoneAsync(request.EventId, request.ZoneId);
            return Json(new { success = removed });
        }

        [HttpGet]
        public async Task<IActionResult> Manage(Guid id)
        {
            var ev = await eventService.GetEventByIdAsync(id);
            if (ev == null) return NotFound();

            var zones = (await zoneService.GetByRoomAsync(ev.RoomId))
                .Where(z => z.IsActive)
                .ToList();
            var tiers = (await pricingTierService.GetByEventAsync(id)).ToList();
            var tiersByZone = tiers.ToDictionary(t => t.ZoneId);
            var seats = (await seatService.GetByRoomAsync(ev.RoomId))
                .Where(s => s.IsActive)
                .ToList();
            var tickets = (await ticketService.GetByEventForAdminAsync(id)).ToList();

            var activeTicketsBySeat = tickets
                .Where(t => t.SeatId != Guid.Empty && t.Status != TicketStatus.Cancelled && t.Status != TicketStatus.Refunded)
                .GroupBy(t => t.SeatId)
                .ToDictionary(g => g.Key, g => g.First());

            var basePrice = (float)ev.BasePrice;
            var seatDtos = seats.Select(s =>
            {
                tiersByZone.TryGetValue(s.ZoneId ?? Guid.Empty, out var tier);
                var zone = zones.FirstOrDefault(z => z.Id == s.ZoneId);
                activeTicketsBySeat.TryGetValue(s.Id, out var ticket);

                return new ManagedSeatDto
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
                    TicketStatus = ticket?.Status,
                    TicketId = ticket?.Id,
                    BuyerDisplay = ticket?.BuyerDisplay
                };
            }).ToList();

            var zoneDtos = zones
                .OrderBy(z => z.DisplayOrder).ThenBy(z => z.Name)
                .Select(z =>
                {
                    var liveSeatCount = seats.Count(s => s.ZoneId == z.Id);
                    var soldCount = seatDtos.Count(s => s.ZoneId == z.Id
                        && s.TicketStatus.HasValue
                        && s.TicketStatus.Value != TicketStatus.Cancelled
                        && s.TicketStatus.Value != TicketStatus.Refunded);
                    tiersByZone.TryGetValue(z.Id, out var tier);
                    return new ManagedZoneDto
                    {
                        Id = z.Id,
                        Name = z.Name ?? "Unnamed Zone",
                        ZoneType = z.ZoneType,
                        SeatCount = liveSeatCount,
                        SoldCount = soldCount,
                        Price = tier?.Price,
                        Currency = tier?.Currency
                    };
                })
                .ToList();

            var soldSeats = seatDtos.Count(s => s.TicketStatus == TicketStatus.Purchased
                || s.TicketStatus == TicketStatus.Used);
            var reservedSeats = seatDtos.Count(s => s.TicketStatus == TicketStatus.Reserved);

            var unzonedSeatCount = seats.Count(s => !s.ZoneId.HasValue);
            var unzonedSoldCount = seatDtos.Count(s => !s.ZoneId.HasValue
                && s.TicketStatus.HasValue
                && s.TicketStatus.Value != TicketStatus.Cancelled
                && s.TicketStatus.Value != TicketStatus.Refunded);

            var soldOrUsed = tickets.Where(t => t.Status == TicketStatus.Purchased || t.Status == TicketStatus.Used);
            var revenue = soldOrUsed.Sum(t => t.Price);
            var currency = tiers.FirstOrDefault()?.Currency ?? "EUR";

            int gridRows = seats.Count > 0 ? seats.Max(s => s.Row) + 1 : 10;
            int gridCols = seats.Count > 0 ? seats.Max(s => s.Column) + 1 : 10;

            var model = new EventTicketsManageViewModel
            {
                EventId = ev.Id,
                EventName = ev.EventName,
                RoomId = ev.RoomId,
                RoomName = ev.RoomName,
                EventStart = ev.StartDateTime,
                GridRows = gridRows,
                GridColumns = gridCols,
                Zones = zoneDtos,
                Seats = seatDtos,
                Tickets = tickets,
                SoldSeats = soldSeats,
                ReservedSeats = reservedSeats,
                TotalRevenue = revenue,
                Currency = currency,
                BasePrice = basePrice,
                UnzonedSeatCount = unzonedSeatCount,
                UnzonedSoldCount = unzonedSoldCount,
                HasZonesWithoutTier = zoneDtos.Any(z => !z.HasPricing)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RefundTicket([FromBody] RefundTicketRequest request)
        {
            var processedBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ok = await ticketService.AdminRefundTicketAsync(request.TicketId, processedBy);
            return Json(new { success = ok });
        }
    }
}
