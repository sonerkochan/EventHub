using EventHub.Core.Models.Zone;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventHub.Core.Contracts
{
    public interface IZoneService
    {
        Task<Guid> CreateAsync(CreateZoneViewModel model, Guid createdBy);
        Task<IEnumerable<ZoneListViewModel>> GetByRoomAsync(Guid roomId);
        Task<EditZoneViewModel?> GetForEditAsync(Guid id);
        Task<bool> UpdateAsync(EditZoneViewModel model);
        Task<bool> DeactivateAsync(Guid id);
        Task<bool> ActivateAsync(Guid id);
    }
}
