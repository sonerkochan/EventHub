using EventHub.Core.Models.Event;
using EventHub.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Core.Contracts
{
    public interface IEventService
    {
        Task<Guid> CreateAsync(CreateEventViewModel model, Guid createdBy);
        Task<IEnumerable<EventListViewModel>> GetAllEventsAsync();
        Task<EventDetailViewModel?> GetEventByIdAsync(Guid id);
        Task<EditEventViewModel?> GetEventForEditAsync(Guid id);
        Task<bool> UpdateAsync(EditEventViewModel model);
        Task<bool> DeactivateAsync(Guid id);
    }
}
