using EventHub.Core.Models.Event;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventHub.Core.Contracts
{
    public interface IEventService
    {
        Task<Guid> CreateAsync(CreateEventViewModel model, Guid createdBy);
        Task<IEnumerable<EventListViewModel>> GetAllEventsAsync();
        Task<IEnumerable<EventListViewModel>> GetPublishedEventsAsync();
        Task<EventDetailViewModel?> GetEventByIdAsync(Guid id);
        Task<EventDetailViewModel?> GetPublishedEventByIdAsync(Guid id);
        Task<EditEventViewModel?> GetEventForEditAsync(Guid id);
        Task<bool> UpdateAsync(EditEventViewModel model);
        Task<bool> DeactivateAsync(Guid id);
        Task<bool> PublishAsync(Guid id);
    }
}
    