using EventHub.Core.Contracts;
using EventHub.Core.Models.Ticket;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
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

        public TicketService(IRepository _repo)
        {
            repo = _repo;
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
                    Price = (float)ev.BasePrice,
                    Currency = "USD",
                    HashedCode = GenerateHashedCode(userId, eventId, nextNumber + i),
                    IsUsed = false,
                    ReservedAt = DateTime.UtcNow,
                    ReservationExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    PurchasedAt = DateTime.UtcNow,
                    ValidatedAt = default
                };

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
            return await repo.AllReadonly<Ticket>()
                .Where(t => t.UserId == userId)
                .Join(
                    repo.AllReadonly<Event>(),
                    t => t.EventId,
                    e => e.Id,
                    (t, e) => new { t, e })
                .Join(
                    repo.AllReadonly<Room>(),
                    te => te.e.RoomId,
                    r => r.RoomId,
                    (te, r) => new TicketListViewModel
                    {
                        Id = te.t.Id,
                        TicketNumber = te.t.TicketNumber,
                        EventName = te.e.EventName!,
                        EventStart = te.e.StartDateTime,
                        RoomName = r.Name!,
                        Price = te.t.Price,
                        Currency = te.t.Currency ?? "USD",
                        IsUsed = te.t.IsUsed,
                        PurchasedAt = te.t.PurchasedAt
                    })
                .OrderByDescending(t => t.PurchasedAt)
                .ToListAsync();
        }

        public async Task<TicketDetailViewModel?> GetTicketByIdAsync(Guid ticketId, Guid userId)
        {
            return await repo.AllReadonly<Ticket>()
                .Where(t => t.Id == ticketId && t.UserId == userId)
                .Join(
                    repo.AllReadonly<Event>(),
                    t => t.EventId,
                    e => e.Id,
                    (t, e) => new { t, e })
                .Join(
                    repo.AllReadonly<Room>(),
                    te => te.e.RoomId,
                    r => r.RoomId,
                    (te, r) => new TicketDetailViewModel
                    {
                        Id = te.t.Id,
                        TicketNumber = te.t.TicketNumber,
                        HashedCode = te.t.HashedCode!,
                        EventName = te.e.EventName!,
                        EventDescription = te.e.Description,
                        EventStart = te.e.StartDateTime,
                        EventEnd = te.e.EndDateTime,
                        RoomName = r.Name!,
                        Price = te.t.Price,
                        Currency = te.t.Currency ?? "USD",
                        IsUsed = te.t.IsUsed,
                        PurchasedAt = te.t.PurchasedAt
                    })
                .FirstOrDefaultAsync();
        }

        private static string GenerateHashedCode(Guid userId, Guid eventId, long ticketNumber)
        {
            var raw = $"{userId}-{eventId}-{ticketNumber}-{DateTime.UtcNow.Ticks}";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes)[..16].ToUpperInvariant();
        }
    }
}