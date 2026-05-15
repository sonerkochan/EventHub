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
                BasePrice = model.BasePrice,
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
            return await BuildEventListQuery(repo.AllReadonly<Event>().Where(e => e.IsActive))
                .ToListAsync();
        }

        public async Task<IEnumerable<EventListViewModel>> GetOrganizersEventsAsync(Guid userId)
        {
            return await BuildEventListQuery(repo.AllReadonly<Event>().Where(e => e.IsActive && e.OrganizerId==userId))
                .ToListAsync();
        }
        
        public async Task<IEnumerable<EventListViewModel>> GetPublishedEventsAsync()
        {
            return await BuildEventListQuery(
                    repo.AllReadonly<Event>()
                        .Where(e => e.IsActive && e.EventStatus == EventStatus.Published))
                .ToListAsync();
        }

        public async Task<EventDetailViewModel?> GetEventByIdAsync(Guid id)
        {
            return await BuildEventDetailQuery(repo.AllReadonly<Event>().Where(e => e.Id == id))
                .FirstOrDefaultAsync();
        }

        public async Task<EventDetailViewModel?> GetPublishedEventByIdAsync(Guid id)
        {
            return await BuildEventDetailQuery(
                    repo.AllReadonly<Event>()
                        .Where(e => e.Id == id && e.IsActive && e.EventStatus == EventStatus.Published))
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
                BasePrice = ev.BasePrice,
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
            ev.BasePrice = model.BasePrice;
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
                        EventType = e.EventType,
                        EventStatus = e.EventStatus,
                        EventPriority = e.EventPriority,
                        StartDateTime = e.StartDateTime,
                        EndDateTime = e.EndDateTime,
                        TotalTickets = e.TotalTickets,
                        TicketsSold = e.TicketsSold,
                        BasePrice = e.BasePrice,
                        IsActive = e.IsActive,
                        RoomName = r.Name,
                        CoverImageUrl = e.CoverImageUrl
                    })
                .OrderBy(e => e.StartDateTime);
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
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt
                    });
        }
    }
}
