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