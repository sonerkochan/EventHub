using EventHub.Core.Contracts;
using EventHub.Core.Models.Ticket;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System;
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

        public TicketService(IRepository _repo, IQRCodeService _qrCodeService)
        {
            repo = _repo;
            qrCodeService = _qrCodeService;
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
                    ticket.QRCodeImage = qrCodeService.GenerateQRCode(ticket.HashedCode!);
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

        private static string GenerateHashedCode(Guid userId, Guid eventId, long ticketNumber)
        {
            var raw = $"{userId}-{eventId}-{ticketNumber}-{DateTime.UtcNow.Ticks}";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes)[..16].ToUpperInvariant();
        }
    }
}