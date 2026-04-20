using EventHub.Core.Models.Seat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventHub.Core.Contracts
{
    public interface ISeatService
    {
        Task<Guid> CreateAsync(CreateSeatViewModel model);
        Task<IEnumerable<Guid>> CreateBatchAsync(Guid roomId, int count, Guid? zoneId);
        Task<IEnumerable<SeatListViewModel>> GetByRoomAsync(Guid roomId);
        Task<EditSeatViewModel?> GetForEditAsync(Guid id);
        Task<bool> UpdateAsync(EditSeatViewModel model);
        Task<bool> DeactivateAsync(Guid id);
        Task<bool> ActivateAsync(Guid id);
        Task<bool> AssignToZoneAsync(Guid seatId, Guid zoneId);
        Task<bool> RemoveFromZoneAsync(Guid seatId);
    }
}
