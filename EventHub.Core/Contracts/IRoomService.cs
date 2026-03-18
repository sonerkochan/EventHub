using EventHub.Core.Models.Room;
using EventHub.Core.Models.Venue;
using EventHub.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Core.Contracts
{
    public interface IRoomService
    {
        Task <Guid> AddRoomAsync(AddRoomViewModel room, Guid userId);
        Task<IEnumerable<RoomListViewModel>> GetAllRoomsAsync();
        Task DeleteRoomAsync(Room room); //?
        Task UpdateRoomAsync(Room room); //?
        Task <Room> GetSingleRoomById(Guid roomId);
    }
}
