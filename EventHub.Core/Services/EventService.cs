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

        public EventService(IRepository _repo)
        {
            repo = _repo;
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
                AllowRefunds = model.AllowRefunds,
                RefundDeadline = model.RefundDeadline ?? default,
                IsActive = true,
                CoverImageUrl = model.CoverImageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<IEnumerable<EventListViewModel>> GetAllEventsAsync()
        {
            return await repo.AllReadonly<Event>()
                .Where(e => e.IsActive)
                .Join(
                    repo.AllReadonly<Room>(),
                    e => e.RoomId,
                    r => r.RoomId,
                    (e, r) => new EventListViewModel
                    {
                        Id = e.Id,
                        EventName = e.EventName!,
                        EventType = e.EventType,
                        EventStatus = e.EventStatus,
                        EventPriority = e.EventPriority,
                        StartDateTime = e.StartDateTime,
                        EndDateTime = e.EndDateTime,
                        TotalTickets = e.TotalTickets,
                        TicketsSold = e.TicketsSold,
                        IsActive = e.IsActive,
                        RoomName = r.Name
                    })
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();
        }

        public async Task<EventDetailViewModel?> GetEventByIdAsync(Guid id)
        {
            return await repo.AllReadonly<Event>()
                .Where(e => e.Id == id)
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
                        AllowRefunds = e.AllowRefunds,
                        RefundDeadline = e.RefundDeadline == default ? null : e.RefundDeadline,
                        IsActive = e.IsActive,
                        CoverImageUrl = e.CoverImageUrl,
                        RoomName = r.Name,
                        RoomId = e.RoomId,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt
                    })
                .FirstOrDefaultAsync();
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
                AllowRefunds = ev.AllowRefunds,
                RefundDeadline = ev.RefundDeadline == default ? null : ev.RefundDeadline,
                CoverImageUrl = ev.CoverImageUrl
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
            ev.AllowRefunds = model.AllowRefunds;
            ev.RefundDeadline = model.RefundDeadline ?? default;
            ev.CoverImageUrl = model.CoverImageUrl;
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
    }
}
