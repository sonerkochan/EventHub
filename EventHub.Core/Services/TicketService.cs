using EventHub.Core.Contracts;
using EventHub.Core.Models.Admin;
using EventHub.Core.Models.Ticket;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using DataEvent = EventHub.Infrastructure.Data.Models.Event;

namespace EventHub.Core.Services
{
    public class TicketService : ITicketService
    {
        private readonly IRepository repo;
        private readonly IQRCodeService qrCodeService;
        private readonly IHttpContextAccessor _http;

        public TicketService(IRepository _repo, IQRCodeService _qrCodeService, IHttpContextAccessor http)
        {
            repo = _repo;
            qrCodeService = _qrCodeService;
            _http = http;
        }

        public async Task<List<Guid>> PurchaseAsync(Guid eventId, Guid userId, int quantity)
        {
            var ev = await repo.All<DataEvent>()
                .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive);

            if (ev == null) return new List<Guid>();

            var remaining = ev.TotalTickets - ev.TicketsSold;
            if (remaining < quantity) return new List<Guid>();

            var lastTicket = await repo.AllReadonly<Ticket>()
                .OrderByDescending(t => t.TicketNumber)
                .FirstOrDefaultAsync();

            long nextNumber = (lastTicket?.TicketNumber ?? 1_000_000) + 1;
            var createdIds = new List<Guid>();


            for (int i = 0; i < quantity; i++)
            {
                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    UserId = userId,
                    SeatId = Guid.Empty,
                    PricingTierId = Guid.Empty,
                    ValidatedBy = Guid.Empty,
                    TicketNumber = nextNumber + i,
                    Status = TicketStatus.Purchased,
                    Price = (float)ev.BasePrice,
                    Currency = "EUR",
                    HashedCode = GenerateHashedCode(userId, eventId, nextNumber + i),
                    IsUsed = false,
                    ReservedAt = DateTime.UtcNow,
                    ReservationExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    PurchasedAt = DateTime.UtcNow,
                    ValidatedAt = default
                };

                try
                {
                    var request = _http.HttpContext!.Request;
                    var baseUrl = $"{request.Scheme}://{request.Host}";
                    var validationUrl = $"{baseUrl}/validate/{ticket.HashedCode}";
                    ticket.QRCodeImage = qrCodeService.GenerateQRCode(validationUrl);
                }
                catch
                {
                    ticket.QRCodeImage = null;
                }

                await repo.AddAsync(ticket);
                createdIds.Add(ticket.Id);
            }
            
            ev.TicketsSold += quantity;
            ev.UpdatedAt = DateTime.UtcNow;
            repo.Update(ev);

            await repo.SaveChangesAsync();
            return createdIds;
        }

        public async Task<ReserveSeatsResult> ReserveSeatsAsync(Guid eventId, Guid userId, IReadOnlyList<Guid> seatIds, TimeSpan? hold = null)
        {
            var result = new ReserveSeatsResult();

            if (seatIds == null || seatIds.Count == 0)
            {
                result.ErrorMessage = "No seats selected.";
                return result;
            }

            if (seatIds.Distinct().Count() != seatIds.Count)
            {
                result.ErrorMessage = "Duplicate seats in selection.";
                return result;
            }

            var ev = await repo.AllReadonly<DataEvent>()
                .Where(e => e.Id == eventId && e.IsActive)
                .Select(e => new { e.Id, e.RoomId, e.BasePrice })
                .FirstOrDefaultAsync();
            if (ev == null)
            {
                result.ErrorMessage = "Event not found.";
                return result;
            }

            var seats = await repo.AllReadonly<Seat>()
                .Where(s => seatIds.Contains(s.Id) && s.RoomId == ev.RoomId && s.IsActive)
                .ToListAsync();

            if (seats.Count != seatIds.Count)
            {
                result.ErrorMessage = "One or more seats are no longer available.";
                return result;
            }

            var takenStatuses = new[] { TicketStatus.Reserved, TicketStatus.Purchased, TicketStatus.Used };
            var nowUtc = DateTime.UtcNow;

            var conflicting = await repo.AllReadonly<Ticket>()
                .Where(t => t.EventId == eventId
                    && seatIds.Contains(t.SeatId)
                    && takenStatuses.Contains(t.Status)
                    && (t.Status != TicketStatus.Reserved || t.ReservationExpiresAt > nowUtc))
                .Select(t => t.SeatId)
                .ToListAsync();

            if (conflicting.Count > 0)
            {
                result.ErrorMessage = "Some of the seats you picked were just taken. Please choose different seats.";
                return result;
            }

            var tiers = await repo.AllReadonly<EventPricingTier>()
                .Where(t => t.EventId == eventId && t.IsActive)
                .ToListAsync();
            var tierByZone = tiers.ToDictionary(t => t.ZoneId);

            var zoneIds = seats.Where(s => s.ZoneId.HasValue).Select(s => s.ZoneId!.Value).Distinct().ToList();
            var zones = await repo.AllReadonly<Zone>()
                .Where(z => zoneIds.Contains(z.Id))
                .ToListAsync();
            var zoneById = zones.ToDictionary(z => z.Id);

            var lastTicket = await repo.AllReadonly<Ticket>()
                .OrderByDescending(t => t.TicketNumber)
                .FirstOrDefaultAsync();
            long nextNumber = (lastTicket?.TicketNumber ?? 1_000_000) + 1;

            var basePrice = (float)ev.BasePrice;
            var holdSpan = hold ?? TimeSpan.FromMinutes(15);

            for (int i = 0; i < seats.Count; i++)
            {
                var seat = seats[i];
                EventPricingTier? tier = null;
                if (seat.ZoneId.HasValue) tierByZone.TryGetValue(seat.ZoneId.Value, out tier);

                var price = tier?.Price ?? basePrice;
                var currency = tier?.Currency ?? "EUR";
                Zone? zone = null;
                if (seat.ZoneId.HasValue) zoneById.TryGetValue(seat.ZoneId.Value, out zone);

                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    UserId = userId,
                    SeatId = seat.Id,
                    PricingTierId = tier?.Id ?? Guid.Empty,
                    ValidatedBy = Guid.Empty,
                    TicketNumber = nextNumber + i,
                    Status = TicketStatus.Reserved,
                    Price = price,
                    Currency = currency,
                    HashedCode = GenerateHashedCode(userId, eventId, nextNumber + i),
                    IsUsed = false,
                    ReservedAt = nowUtc,
                    ReservationExpiresAt = nowUtc.Add(holdSpan),
                    PurchasedAt = default,
                    ValidatedAt = default
                };

                await repo.AddAsync(ticket);

                result.TicketIds.Add(ticket.Id);
                result.Lines.Add(new ReservedSeatLine
                {
                    TicketId = ticket.Id,
                    SeatId = seat.Id,
                    SeatNumber = seat.SeatNumber,
                    ZoneName = zone?.Name,
                    Price = price,
                    Currency = currency
                });
                result.TotalPrice += price;
                result.Currency = currency;
            }

            await repo.SaveChangesAsync();

            result.Success = true;
            return result;
        }

        public async Task<bool> ConfirmReservedTicketsAsync(IReadOnlyList<Guid> ticketIds)
        {
            if (ticketIds == null || ticketIds.Count == 0) return false;

            var tickets = await repo.All<Ticket>()
                .Where(t => ticketIds.Contains(t.Id))
                .ToListAsync();

            if (tickets.Count == 0) return false;

            var nowUtc = DateTime.UtcNow;
            var tierIds = tickets.Where(t => t.PricingTierId != Guid.Empty)
                .Select(t => t.PricingTierId).Distinct().ToList();
            var tiers = await repo.All<EventPricingTier>()
                .Where(t => tierIds.Contains(t.Id))
                .ToListAsync();
            var tierById = tiers.ToDictionary(t => t.Id);

            var ticketsPerEvent = new Dictionary<Guid, int>();

            int flipped = 0;
            foreach (var ticket in tickets)
            {
                if (ticket.Status != TicketStatus.Reserved) continue;

                ticket.Status = TicketStatus.Purchased;
                ticket.PurchasedAt = nowUtc;

                try
                {
                    var request = _http.HttpContext?.Request;
                    if (request != null)
                    {
                        var baseUrl = $"{request.Scheme}://{request.Host}";
                        var validationUrl = $"{baseUrl}/validate/{ticket.HashedCode}";
                        ticket.QRCodeImage = qrCodeService.GenerateQRCode(validationUrl);
                    }
                }
                catch
                {
                    // QR generation is best-effort; ticket is still valid without it.
                }

                repo.Update(ticket);

                if (ticket.PricingTierId != Guid.Empty
                    && tierById.TryGetValue(ticket.PricingTierId, out var tier))
                {
                    tier.SoldQuantity += 1;
                    tier.UpdatedAt = nowUtc;
                    repo.Update(tier);
                }

                ticketsPerEvent[ticket.EventId] = ticketsPerEvent.GetValueOrDefault(ticket.EventId) + 1;

                flipped++;
            }

            if (flipped == 0) return false;

            await repo.SaveChangesAsync();

            // Bump Event.TicketsSold without loading the row
            foreach (var (eventId, increment) in ticketsPerEvent)
            {
                await repo.All<DataEvent>()
                    .Where(e => e.Id == eventId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(e => e.TicketsSold, e => e.TicketsSold + increment)
                        .SetProperty(e => e.UpdatedAt, nowUtc));
            }

            return true;
        }

        public async Task<TicketValidationResult?> ValidateTicketAsync(string hashedCode)
        {
            var ticket = await repo.All<Ticket>()
                .FirstOrDefaultAsync(t => t.HashedCode == hashedCode);

            if (ticket == null) return null;

            var ev = await repo.AllReadonly<DataEvent>()
                .FirstOrDefaultAsync(e => e.Id == ticket.EventId);

            var room = await repo.AllReadonly<Room>()
                .FirstOrDefaultAsync(r => r.RoomId == ev!.RoomId);

            var user = await repo.AllReadonly<User>()
                .FirstOrDefaultAsync(u => u.Id == ticket.UserId.ToString());

            bool wasAlreadyUsed = ticket.IsUsed;

            if (!ticket.IsUsed)
            {
                ticket.IsUsed = true;
                ticket.Status = TicketStatus.Used;
                ticket.ValidatedAt = DateTime.UtcNow;
                repo.Update(ticket);
                await repo.SaveChangesAsync();
            }

            return new TicketValidationResult
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                EventName = ev!.EventName!,
                EventStart = ev.StartDateTime,
                RoomName = room?.Name ?? "Unknown",
                UserFullName = $"{user?.FirstName} {user?.LastName}".Trim(),
                UserEmail = user?.Email ?? "Unknown",
                Price = ticket.Price,
                Currency = ticket.Currency ?? "EUR",
                WasAlreadyUsed = wasAlreadyUsed,
                UsedAt = ticket.ValidatedAt == default ? null : ticket.ValidatedAt
            };
        }

        public async Task<IEnumerable<TicketListViewModel>> GetUserTicketsAsync(Guid userId)
        {
            var tickets = await repo.AllReadonly<Ticket>()
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var eventIds = tickets.Select(t => t.EventId).Distinct().ToList();
            var events = await repo.AllReadonly<Event>()
                .Where(e => eventIds.Contains(e.Id))
                .ToListAsync();

            var roomIds = events.Select(e => e.RoomId).Distinct().ToList();
            var rooms = await repo.AllReadonly<Room>()
                .Where(r => roomIds.Contains(r.RoomId))
                .ToListAsync();

            var result = tickets.Select(t =>
            {
                var evt = events.FirstOrDefault(e => e.Id == t.EventId);
                var room = rooms.FirstOrDefault(r => r.RoomId == evt?.RoomId);
                return new TicketListViewModel
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    EventName = evt?.EventName ?? "Unknown Event",
                    EventStart = evt?.StartDateTime ?? DateTime.MinValue,
                    RoomName = room?.Name ?? "Unknown Room",
                    Price = t.Price,
                    Currency = t.Currency ?? "EUR",
                    IsUsed = t.IsUsed,
                    PurchasedAt = t.PurchasedAt,
                    Status = t.Status
                };
            }).OrderByDescending(t => t.PurchasedAt);

            return result;
        }

        public async Task<TicketDetailViewModel?> GetTicketByIdAsync(Guid ticketId, Guid userId)
        {
            var ticket = await repo.AllReadonly<Ticket>()
                .FirstOrDefaultAsync(t => t.Id == ticketId && t.UserId == userId);

            if (ticket == null)
                return null;

            var evt = await repo.AllReadonly<Event>()
                .FirstOrDefaultAsync(e => e.Id == ticket.EventId);

            if (evt == null)
                return null;

            var room = await repo.AllReadonly<Room>()
                .FirstOrDefaultAsync(r => r.RoomId == evt.RoomId);

            return new TicketDetailViewModel
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                HashedCode = ticket.HashedCode!,
                QRCodeImage = ticket.QRCodeImage,
                EventName = evt.EventName!,
                EventDescription = evt.Description,
                EventStart = evt.StartDateTime,
                EventEnd = evt.EndDateTime,
                RoomName = room?.Name ?? "Unknown Room",
                Price = ticket.Price,
                Currency = ticket.Currency ?? "EUR",
                IsUsed = ticket.IsUsed,
                PurchasedAt = ticket.PurchasedAt
            };
        }

        public async Task<IEnumerable<AdminTicketRow>> GetByEventForAdminAsync(Guid eventId)
        {
            var tickets = await repo.AllReadonly<Ticket>()
                .Where(t => t.EventId == eventId)
                .OrderByDescending(t => t.ReservedAt)
                .ToListAsync();

            if (tickets.Count == 0) return new List<AdminTicketRow>();

            var seatIds = tickets.Select(t => t.SeatId).Where(id => id != Guid.Empty).Distinct().ToList();
            var seats = await repo.AllReadonly<Seat>()
                .Where(s => seatIds.Contains(s.Id))
                .ToListAsync();
            var seatById = seats.ToDictionary(s => s.Id);

            var zoneIds = seats.Where(s => s.ZoneId.HasValue).Select(s => s.ZoneId!.Value).Distinct().ToList();
            var zones = await repo.AllReadonly<Zone>()
                .Where(z => zoneIds.Contains(z.Id))
                .ToListAsync();
            var zoneById = zones.ToDictionary(z => z.Id);

            var userIdStrings = tickets.Select(t => t.UserId.ToString()).Distinct().ToList();
            var users = await repo.AllReadonly<User>()
                .Where(u => userIdStrings.Contains(u.Id))
                .ToListAsync();
            var userById = users.ToDictionary(u => u.Id);

            return tickets.Select(t =>
            {
                seatById.TryGetValue(t.SeatId, out var seat);
                Zone? zone = null;
                if (seat?.ZoneId.HasValue == true) zoneById.TryGetValue(seat.ZoneId.Value, out zone);
                userById.TryGetValue(t.UserId.ToString(), out var user);

                var buyerDisplay = user == null
                    ? t.UserId.ToString()
                    : (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
                        ? $"{user.FirstName} {user.LastName}".Trim()
                        : (user.UserName ?? user.Email ?? t.UserId.ToString());

                return new AdminTicketRow
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    HashedCode = t.HashedCode,
                    SeatId = t.SeatId,
                    SeatNumber = seat?.SeatNumber ?? 0,
                    ZoneId = seat?.ZoneId,
                    ZoneName = zone?.Name,
                    Status = t.Status,
                    Price = t.Price,
                    Currency = t.Currency,
                    BuyerUserId = t.UserId,
                    BuyerDisplay = buyerDisplay!,
                    ReservedAt = t.ReservedAt,
                    PurchasedAt = t.PurchasedAt
                };
            }).ToList();
        }

        public async Task<IEnumerable<AdminTicketRow>> GetAllForAdminAsync(TicketStatus? statusFilter = null)
        {
            var query = repo.AllReadonly<Ticket>().AsQueryable();
            if (statusFilter.HasValue)
            {
                query = query.Where(t => t.Status == statusFilter.Value);
            }

            var tickets = await query.ToListAsync();
            if (tickets.Count == 0) return new List<AdminTicketRow>();

            var eventIds = tickets.Select(t => t.EventId).Distinct().ToList();
            var events = await repo.AllReadonly<DataEvent>()
                .Where(e => eventIds.Contains(e.Id))
                .ToListAsync();
            var eventById = events.ToDictionary(e => e.Id);

            var seatIds = tickets.Select(t => t.SeatId).Where(id => id != Guid.Empty).Distinct().ToList();
            var seats = await repo.AllReadonly<Seat>()
                .Where(s => seatIds.Contains(s.Id))
                .ToListAsync();
            var seatById = seats.ToDictionary(s => s.Id);

            var zoneIds = seats.Where(s => s.ZoneId.HasValue).Select(s => s.ZoneId!.Value).Distinct().ToList();
            var zones = await repo.AllReadonly<Zone>()
                .Where(z => zoneIds.Contains(z.Id))
                .ToListAsync();
            var zoneById = zones.ToDictionary(z => z.Id);

            var userIdStrings = tickets.Select(t => t.UserId.ToString()).Distinct().ToList();
            var users = await repo.AllReadonly<User>()
                .Where(u => userIdStrings.Contains(u.Id))
                .ToListAsync();
            var userById = users.ToDictionary(u => u.Id);

            var rows = tickets.Select(t =>
            {
                eventById.TryGetValue(t.EventId, out var ev);
                seatById.TryGetValue(t.SeatId, out var seat);
                Zone? zone = null;
                if (seat?.ZoneId.HasValue == true) zoneById.TryGetValue(seat.ZoneId.Value, out zone);
                userById.TryGetValue(t.UserId.ToString(), out var user);

                var buyerDisplay = user == null
                    ? t.UserId.ToString()
                    : (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
                        ? $"{user.FirstName} {user.LastName}".Trim()
                        : (user.UserName ?? user.Email ?? t.UserId.ToString());

                return new AdminTicketRow
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    HashedCode = t.HashedCode,
                    EventId = t.EventId,
                    EventName = ev?.EventName,
                    EventStart = ev?.StartDateTime ?? DateTime.MinValue,
                    SeatId = t.SeatId,
                    SeatNumber = seat?.SeatNumber ?? 0,
                    ZoneId = seat?.ZoneId,
                    ZoneName = zone?.Name,
                    Status = t.Status,
                    Price = t.Price,
                    Currency = t.Currency,
                    BuyerUserId = t.UserId,
                    BuyerDisplay = buyerDisplay!,
                    ReservedAt = t.ReservedAt,
                    PurchasedAt = t.PurchasedAt
                };
            }).ToList();

            return rows
                .OrderByDescending(r => r.EventStart)
                .ThenBy(r => r.EventName)
                .ThenByDescending(r => r.TicketNumber)
                .ToList();
        }

        public Task<AdminTicketLookupDto?> LookupByNumberAsync(long ticketNumber)
            => BuildLookupAsync(t => t.TicketNumber == ticketNumber);

        public Task<AdminTicketLookupDto?> LookupAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Task.FromResult<AdminTicketLookupDto?>(null);

            var trimmed = query.Trim();
            if (long.TryParse(trimmed, out var number))
            {
                return BuildLookupAsync(t => t.TicketNumber == number);
            }

            var code = trimmed.ToUpperInvariant();
            return BuildLookupAsync(t => t.HashedCode != null && t.HashedCode.ToUpper() == code);
        }

        private async Task<AdminTicketLookupDto?> BuildLookupAsync(System.Linq.Expressions.Expression<Func<Ticket, bool>> predicate)
        {
            var ticket = await repo.AllReadonly<Ticket>()
                .FirstOrDefaultAsync(predicate);

            if (ticket == null) return null;

            var ev = await repo.AllReadonly<DataEvent>()
                .FirstOrDefaultAsync(e => e.Id == ticket.EventId);
            if (ev == null) return null;

            var room = await repo.AllReadonly<Room>()
                .FirstOrDefaultAsync(r => r.RoomId == ev.RoomId);

            Seat? seat = null;
            Zone? zone = null;
            if (ticket.SeatId != Guid.Empty)
            {
                seat = await repo.AllReadonly<Seat>()
                    .FirstOrDefaultAsync(s => s.Id == ticket.SeatId);
                if (seat?.ZoneId != null)
                {
                    zone = await repo.AllReadonly<Zone>()
                        .FirstOrDefaultAsync(z => z.Id == seat.ZoneId);
                }
            }

            var user = await repo.AllReadonly<User>()
                .FirstOrDefaultAsync(u => u.Id == ticket.UserId.ToString());

            var buyerDisplay = user == null
                ? ticket.UserId.ToString()
                : (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
                    ? $"{user.FirstName} {user.LastName}".Trim()
                    : (user.UserName ?? user.Email ?? ticket.UserId.ToString());

            return new AdminTicketLookupDto
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                HashedCode = ticket.HashedCode,
                EventId = ev.Id,
                EventName = ev.EventName!,
                EventStart = ev.StartDateTime,
                RoomName = room?.Name,
                SeatNumber = seat?.SeatNumber ?? 0,
                ZoneName = zone?.Name,
                BuyerDisplay = buyerDisplay!,
                BuyerEmail = user?.Email,
                Status = ticket.Status,
                Price = ticket.Price,
                Currency = ticket.Currency,
                ReservedAt = ticket.ReservedAt,
                PurchasedAt = ticket.PurchasedAt,
                ValidatedAt = ticket.ValidatedAt
            };
        }

        public async Task<AdminTicketEditViewModel?> GetForAdminEditAsync(Guid ticketId)
        {
            var ticket = await repo.AllReadonly<Ticket>()
                .FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return null;

            var ev = await repo.AllReadonly<DataEvent>()
                .FirstOrDefaultAsync(e => e.Id == ticket.EventId);
            if (ev == null) return null;

            Seat? currentSeat = null;
            Zone? currentZone = null;
            if (ticket.SeatId != Guid.Empty)
            {
                currentSeat = await repo.AllReadonly<Seat>()
                    .FirstOrDefaultAsync(s => s.Id == ticket.SeatId);
                if (currentSeat?.ZoneId != null)
                {
                    currentZone = await repo.AllReadonly<Zone>()
                        .FirstOrDefaultAsync(z => z.Id == currentSeat.ZoneId);
                }
            }

            var roomSeats = await repo.AllReadonly<Seat>()
                .Where(s => s.RoomId == ev.RoomId && s.IsActive)
                .ToListAsync();

            var zoneIds = roomSeats.Where(s => s.ZoneId.HasValue).Select(s => s.ZoneId!.Value).Distinct().ToList();
            var roomZones = await repo.AllReadonly<Zone>()
                .Where(z => zoneIds.Contains(z.Id))
                .ToListAsync();
            var zoneById = roomZones.ToDictionary(z => z.Id);

            var blockingStatuses = new[] { TicketStatus.Reserved, TicketStatus.Purchased, TicketStatus.Used };
            var nowUtc = DateTime.UtcNow;
            var takenSeatIds = await repo.AllReadonly<Ticket>()
                .Where(t => t.EventId == ticket.EventId
                    && t.Id != ticket.Id
                    && t.SeatId != Guid.Empty
                    && blockingStatuses.Contains(t.Status)
                    && (t.Status != TicketStatus.Reserved || t.ReservationExpiresAt > nowUtc))
                .Select(t => t.SeatId)
                .ToListAsync();
            var takenSet = takenSeatIds.ToHashSet();

            var availableSeats = roomSeats
                .Where(s => s.Id == ticket.SeatId || !takenSet.Contains(s.Id))
                .OrderBy(s => s.Row).ThenBy(s => s.Column)
                .Select(s =>
                {
                    string? zoneName = null;
                    if (s.ZoneId.HasValue && zoneById.TryGetValue(s.ZoneId.Value, out var z))
                    {
                        zoneName = z.Name;
                    }
                    return new AdminAvailableSeatOption
                    {
                        Id = s.Id,
                        SeatNumber = s.SeatNumber,
                        ZoneName = zoneName,
                        IsCurrent = s.Id == ticket.SeatId
                    };
                })
                .ToList();

            var user = await repo.AllReadonly<User>()
                .FirstOrDefaultAsync(u => u.Id == ticket.UserId.ToString());
            var buyerDisplay = user == null
                ? ticket.UserId.ToString()
                : (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
                    ? $"{user.FirstName} {user.LastName}".Trim()
                    : (user.UserName ?? user.Email ?? ticket.UserId.ToString());

            return new AdminTicketEditViewModel
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                HashedCode = ticket.HashedCode,
                EventId = ev.Id,
                EventName = ev.EventName!,
                BuyerDisplay = buyerDisplay,
                CurrentSeatId = ticket.SeatId,
                CurrentSeatNumber = currentSeat?.SeatNumber ?? 0,
                CurrentZoneName = currentZone?.Name,
                CurrentStatus = ticket.Status,
                Price = ticket.Price,
                Currency = ticket.Currency,
                AvailableSeats = availableSeats
            };
        }

        public async Task<(bool Success, string? Error)> AdminUpdateTicketAsync(AdminTicketEditRequest request)
        {
            var ticket = await repo.All<Ticket>()
                .FirstOrDefaultAsync(t => t.Id == request.TicketId);
            if (ticket == null) return (false, "Ticket not found.");

            var nowUtc = DateTime.UtcNow;
            var oldStatus = ticket.Status;

            if (request.SeatId != Guid.Empty && request.SeatId != ticket.SeatId)
            {
                var newSeat = await repo.AllReadonly<Seat>()
                    .FirstOrDefaultAsync(s => s.Id == request.SeatId && s.IsActive);
                if (newSeat == null) return (false, "Selected seat is no longer available.");

                var ev = await repo.AllReadonly<DataEvent>()
                    .FirstOrDefaultAsync(e => e.Id == ticket.EventId);
                if (ev == null || newSeat.RoomId != ev.RoomId)
                {
                    return (false, "Seat does not belong to this event's room.");
                }

                var blockingStatuses = new[] { TicketStatus.Reserved, TicketStatus.Purchased, TicketStatus.Used };
                var conflict = await repo.AllReadonly<Ticket>()
                    .AnyAsync(t => t.EventId == ticket.EventId
                                && t.Id != ticket.Id
                                && t.SeatId == request.SeatId
                                && blockingStatuses.Contains(t.Status)
                                && (t.Status != TicketStatus.Reserved || t.ReservationExpiresAt > nowUtc));
                if (conflict) return (false, "That seat is already taken.");

                ticket.SeatId = request.SeatId;
            }

            if (request.Status != oldStatus)
            {
                ticket.Status = request.Status;

                if (request.Status == TicketStatus.Purchased && oldStatus != TicketStatus.Used)
                {
                    if (ticket.PurchasedAt == default) ticket.PurchasedAt = nowUtc;
                    ticket.IsUsed = false;
                    ticket.ValidatedAt = default;
                }
                else if (request.Status == TicketStatus.Used)
                {
                    ticket.IsUsed = true;
                    if (ticket.ValidatedAt == default) ticket.ValidatedAt = nowUtc;
                    if (ticket.PurchasedAt == default) ticket.PurchasedAt = nowUtc;
                }
                else if (request.Status == TicketStatus.Reserved)
                {
                    ticket.IsUsed = false;
                    ticket.ValidatedAt = default;
                    if (ticket.ReservedAt == default) ticket.ReservedAt = nowUtc;
                    ticket.ReservationExpiresAt = nowUtc.AddMinutes(15);
                }
                else
                {
                    ticket.IsUsed = false;
                    ticket.ValidatedAt = default;
                }

                if (ticket.PricingTierId != Guid.Empty)
                {
                    var tier = await repo.All<EventPricingTier>()
                        .FirstOrDefaultAsync(t => t.Id == ticket.PricingTierId);
                    if (tier != null)
                    {
                        var wasCounted = oldStatus == TicketStatus.Purchased || oldStatus == TicketStatus.Used;
                        var nowCounted = request.Status == TicketStatus.Purchased || request.Status == TicketStatus.Used;

                        if (wasCounted && !nowCounted && tier.SoldQuantity > 0)
                        {
                            tier.SoldQuantity -= 1;
                            tier.UpdatedAt = nowUtc;
                            repo.Update(tier);
                        }
                        else if (!wasCounted && nowCounted)
                        {
                            tier.SoldQuantity += 1;
                            tier.UpdatedAt = nowUtc;
                            repo.Update(tier);
                        }
                    }
                }
            }

            repo.Update(ticket);
            await repo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> AdminRefundTicketAsync(Guid ticketId, Guid processedBy)
        {
            var ticket = await repo.All<Ticket>().FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return false;

            if (ticket.Status == TicketStatus.Refunded || ticket.Status == TicketStatus.Cancelled)
                return false;

            ticket.Status = TicketStatus.Refunded;

            var payments = await repo.AllReadonly<PaymentTicket>()
                .Where(pt => pt.TicketId == ticketId)
                .Join(repo.All<Payment>(), pt => pt.PaymentId, p => p.Id, (pt, p) => p)
                .ToListAsync();

            foreach (var p in payments)
            {
                if (p.Status != Payment.PaymentStatus.Refunded)
                {
                    p.Status = Payment.PaymentStatus.Refunded;
                    p.RefundedAt = DateTime.UtcNow;
                    p.UpdatedAt = DateTime.UtcNow;
                    repo.Update(p);
                }
            }

            repo.Update(ticket);

            var tier = await repo.All<EventPricingTier>().FirstOrDefaultAsync(t => t.Id == ticket.PricingTierId);
            if (tier != null && tier.SoldQuantity > 0)
            {
                tier.SoldQuantity -= 1;
                tier.UpdatedAt = DateTime.UtcNow;
                repo.Update(tier);
            }

            await repo.SaveChangesAsync();
            return true;
        }

        private static string GenerateHashedCode(Guid userId, Guid eventId, long ticketNumber)
        {
            var raw = $"{userId}-{eventId}-{ticketNumber}-{DateTime.UtcNow.Ticks}";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes)[..16].ToUpperInvariant().Replace("/", "A").Replace("+", "B").Replace("=", "");
        }
    }
}