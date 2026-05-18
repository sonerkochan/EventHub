using EventHub.Core.Contracts;
using EventHub.Core.Models.Event;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Core.Services
{
    public class EventService : IEventService
    {
        private readonly IRepository repo;
        private readonly ICurrencyDisplayService currencyDisplayService;

        public EventService(
            IRepository _repo,
            ICurrencyDisplayService _currencyDisplayService)
        {
            repo = _repo;
            currencyDisplayService = _currencyDisplayService;
        }

        public async Task<Guid> CreateAsync(CreateEventViewModel model, Guid createdBy)
        {
            var entity = new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = createdBy,
                RoomId = model.RoomId,
                EventName = model.EventName,
                Description = model.Description,
                EventType = model.EventType,
                EventPriority = model.EventPriority,
                EventStatus = EventStatus.Draft,
                StartDateTime = model.StartDateTime,
                EndDateTime = model.EndDateTime,
                TotalTickets = model.TotalTickets,
                TicketsSold = 0,
                BasePrice = model.BasePrice,
                AllowRefunds = model.AllowRefunds,
                RefundDeadline = model.RefundDeadline ?? default,
                IsActive = true,
                CoverImageUrl = model.CoverImageUrl,
                Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim(),
                City = string.IsNullOrWhiteSpace(model.City) ? null : model.City.Trim(),
                CountryCode = string.IsNullOrWhiteSpace(model.CountryCode)
                    ? null
                    : model.CountryCode.Trim().ToUpperInvariant(),
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<IEnumerable<EventListViewModel>> GetAllEventsAsync()
        {
            var events = await BuildEventListQuery(repo.AllReadonly<Event>().Where(e => e.IsActive))
                .ToListAsync();

            await ApplyDisplayPricesAsync(events);
            return events;
        }

        public async Task<IEnumerable<EventListViewModel>> GetOrganizersEventsAsync(Guid userId)
        {
            var events = await BuildEventListQuery(repo.AllReadonly<Event>().Where(e => e.IsActive && e.OrganizerId==userId))
                .ToListAsync();

            await ApplyDisplayPricesAsync(events);
            return events;
        }
        
        public async Task<IEnumerable<EventListViewModel>> GetPublishedEventsAsync()
        {
            var events = await BuildEventListQuery(
                    repo.AllReadonly<Event>()
                        .Where(e => e.IsActive && e.EventStatus == EventStatus.Published))
                .ToListAsync();

            await ApplyDisplayPricesAsync(events);
            return events;
        }

        public async Task<IEnumerable<EventListViewModel>> GetPublishedEventsByCityAsync(string city)
        {
            var normalizedCity = city.Trim().ToLowerInvariant();

            var events = await BuildEventListQuery(
                    repo.AllReadonly<Event>()
                        .Where(e =>
                            e.IsActive &&
                            e.EventStatus == EventStatus.Published &&
                            e.City != null &&
                            e.City.ToLower() == normalizedCity))
                .ToListAsync();

            await ApplyDisplayPricesAsync(events);
            return events;
        }

        public async Task<EventDetailViewModel?> GetEventByIdAsync(Guid id)
        {
            var detail = await BuildEventDetailQuery(repo.AllReadonly<Event>().Where(e => e.Id == id))
                .FirstOrDefaultAsync();

            if (detail != null)
            {
                await ApplyDisplayPriceAsync(detail);
            }

            return detail;
        }

        public async Task<EventDetailViewModel?> GetPublishedEventByIdAsync(Guid id)
        {
            var detail = await BuildEventDetailQuery(
                    repo.AllReadonly<Event>()
                        .Where(e => e.Id == id && e.IsActive && e.EventStatus == EventStatus.Published))
                .FirstOrDefaultAsync();

            if (detail != null)
            {
                await ApplyDisplayPriceAsync(detail);
            }

            return detail;
        }

        public async Task<EditEventViewModel?> GetEventForEditAsync(Guid id)
        {
            var ev = await repo.AllReadonly<Event>()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return null;

            return new EditEventViewModel
            {
                Id = ev.Id,
                EventName = ev.EventName!,
                Description = ev.Description,
                EventType = ev.EventType,
                EventStatus = ev.EventStatus,
                EventPriority = ev.EventPriority,
                RoomId = ev.RoomId,
                StartDateTime = ev.StartDateTime,
                EndDateTime = ev.EndDateTime,
                TotalTickets = ev.TotalTickets,
                BasePrice = ev.BasePrice,
                AllowRefunds = ev.AllowRefunds,
                RefundDeadline = ev.RefundDeadline == default ? null : ev.RefundDeadline,
                CoverImageUrl = ev.CoverImageUrl,
                Address = ev.Address,
                City = ev.City,
                CountryCode = ev.CountryCode,
                Latitude = ev.Latitude,
                Longitude = ev.Longitude
            };
        }

        public async Task<bool> UpdateAsync(EditEventViewModel model)
        {
            var ev = await repo.All<Event>()
                .FirstOrDefaultAsync(e => e.Id == model.Id);

            if (ev == null) return false;

            ev.EventName = model.EventName;
            ev.Description = model.Description;
            ev.EventType = model.EventType;
            ev.EventStatus = model.EventStatus;
            ev.EventPriority = model.EventPriority;
            ev.RoomId = model.RoomId;
            ev.StartDateTime = model.StartDateTime;
            ev.EndDateTime = model.EndDateTime;
            ev.TotalTickets = model.TotalTickets;
            ev.BasePrice = model.BasePrice;
            ev.AllowRefunds = model.AllowRefunds;
            ev.RefundDeadline = model.RefundDeadline ?? default;
            ev.CoverImageUrl = model.CoverImageUrl;
            ev.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
            ev.City = string.IsNullOrWhiteSpace(model.City) ? null : model.City.Trim();
            ev.CountryCode = string.IsNullOrWhiteSpace(model.CountryCode)
                ? null
                : model.CountryCode.Trim().ToUpperInvariant();
            ev.Latitude = model.Latitude;
            ev.Longitude = model.Longitude;
            ev.UpdatedAt = DateTime.UtcNow;

            repo.Update(ev);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            var ev = await repo.All<Event>()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return false;

            ev.IsActive = false;
            ev.EventStatus = EventStatus.Cancelled;
            ev.UpdatedAt = DateTime.UtcNow;

            repo.Update(ev);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishAsync(Guid id)
        {
            var ev = await repo.All<Event>()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return false;

            ev.EventStatus = EventStatus.Published;
            ev.UpdatedAt = DateTime.UtcNow;

            repo.Update(ev);
            await repo.SaveChangesAsync();
            return true;
        }

        private IQueryable<EventListViewModel> BuildEventListQuery(IQueryable<Event> source)
        {
            return source
                .Join(
                    repo.AllReadonly<Room>(),
                    e => e.RoomId,
                    r => r.RoomId,
                    (e, r) => new EventListViewModel
                    {
                        Id = e.Id,
                        EventName = e.EventName!,
                        Description = e.Description,
                        EventType = e.EventType,
                        EventStatus = e.EventStatus,
                        EventPriority = e.EventPriority,
                        StartDateTime = e.StartDateTime,
                        EndDateTime = e.EndDateTime,
                        Latitude = e.Latitude,
                        Longitude = e.Longitude,
                        City = e.City,
                        Address = e.Address,
                        CountryCode = e.CountryCode,
                        TotalTickets = e.TotalTickets,
                        TicketsSold = e.TicketsSold,
                        BasePrice = e.BasePrice,
                        IsActive = e.IsActive,
                        RoomName = r.Name,
                        CoverImageUrl = e.CoverImageUrl
                    })
                .OrderBy(e => e.StartDateTime);
        }

        private async Task ApplyDisplayPricesAsync(List<EventListViewModel> events)
        {
            if (events.Count == 0)
            {
                return;
            }

            var eventIds = events.Select(e => e.Id).ToList();
            var priceLookup = await BuildPriceLookupAsync(eventIds);

            foreach (var ev in events)
            {
                await ApplyDisplayPriceAsync(
                    ev.Id,
                    ev.BasePrice,
                    priceLookup,
                    (priceAmount, displayPrice, displayCurrency, priceText, isFree) =>
                    {
                        ev.PriceAmount = priceAmount;
                        ev.DisplayPrice = displayPrice;
                        ev.DisplayCurrency = displayCurrency;
                        ev.PriceText = priceText;
                        ev.IsFree = isFree;
                    });
            }
        }

        private async Task ApplyDisplayPriceAsync(EventDetailViewModel ev)
        {
            var priceLookup = await BuildPriceLookupAsync([ev.Id]);

            await ApplyDisplayPriceAsync(
                ev.Id,
                ev.BasePrice,
                priceLookup,
                (priceAmount, displayPrice, displayCurrency, priceText, isFree) =>
                {
                    ev.PriceAmount = priceAmount;
                    ev.DisplayPrice = displayPrice;
                    ev.DisplayCurrency = displayCurrency;
                    ev.PriceText = priceText;
                    ev.IsFree = isFree;
                });
        }

        private async Task<EventPriceLookup> BuildPriceLookupAsync(IReadOnlyCollection<Guid> eventIds)
        {
            var tierPrices = await repo.AllReadonly<EventPricingTier>()
                .Where(t => t.IsActive && t.Price > 0 && eventIds.Contains(t.EventId))
                .Select(t => new { t.EventId, t.Price })
                .ToListAsync();

            var ticketPrices = await repo.AllReadonly<Ticket>()
                .Where(t =>
                    t.Price > 0 &&
                    eventIds.Contains(t.EventId) &&
                    t.Status != TicketStatus.Cancelled &&
                    t.Status != TicketStatus.Refunded)
                .Select(t => new { t.EventId, t.Price })
                .ToListAsync();

            return new EventPriceLookup
            {
                TierPricesByEvent = tierPrices
                .GroupBy(t => t.EventId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(t => (decimal)t.Price)),
                TicketPricesByEvent = ticketPrices
                    .GroupBy(t => t.EventId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(t => (decimal)t.Price))
            };
        }

        private async Task ApplyDisplayPriceAsync(
            Guid eventId,
            decimal basePrice,
            EventPriceLookup priceLookup,
            Action<decimal?, decimal?, string, string, bool> apply)
        {
            priceLookup.TierPricesByEvent.TryGetValue(eventId, out var tiersForEvent);
            priceLookup.TicketPricesByEvent.TryGetValue(eventId, out var ticketsForEvent);
            var lowestPaidPrice = EventDisplayPriceCalculator.GetLowestPaidPrice(
                basePrice,
                tiersForEvent ?? Enumerable.Empty<decimal>(),
                ticketsForEvent ?? Enumerable.Empty<decimal>());

            if (!lowestPaidPrice.HasValue)
            {
                apply(null, null, "EUR", "Free", true);
                return;
            }

            var displayPrice = await currencyDisplayService.FormatAsync(lowestPaidPrice.Value);
            apply(lowestPaidPrice.Value, displayPrice.Amount, displayPrice.Currency, displayPrice.Text, false);
        }

        private IQueryable<EventDetailViewModel> BuildEventDetailQuery(IQueryable<Event> source)
        {
            return source
                .Join(
                    repo.AllReadonly<Room>(),
                    e => e.RoomId,
                    r => r.RoomId,
                    (e, r) => new EventDetailViewModel
                    {
                        Id = e.Id,
                        EventName = e.EventName!,
                        Description = e.Description,
                        EventType = e.EventType,
                        EventStatus = e.EventStatus,
                        EventPriority = e.EventPriority,
                        StartDateTime = e.StartDateTime,
                        EndDateTime = e.EndDateTime,
                        TotalTickets = e.TotalTickets,
                        TicketsSold = e.TicketsSold,
                        BasePrice = e.BasePrice,
                        AllowRefunds = e.AllowRefunds,
                        RefundDeadline = e.RefundDeadline == default ? null : e.RefundDeadline,
                        IsActive = e.IsActive,
                        CoverImageUrl = e.CoverImageUrl,
                        RoomName = r.Name,
                        RoomId = e.RoomId,
                        Address = e.Address,
                        City = e.City,
                        CountryCode = e.CountryCode,
                        Latitude = e.Latitude,
                        Longitude = e.Longitude,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt
                    });
        }

        private sealed class EventPriceLookup
        {
            public Dictionary<Guid, IEnumerable<decimal>> TierPricesByEvent { get; set; } = new();
            public Dictionary<Guid, IEnumerable<decimal>> TicketPricesByEvent { get; set; } = new();
        }
    }
}
