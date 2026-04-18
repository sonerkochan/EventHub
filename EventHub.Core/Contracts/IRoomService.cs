using EventHub.Core.Models.Room;
using EventHub.Infrastructure.Data.Models;

namespace EventHub.Core.Contracts
{
    public interface IRoomService
    {
        Task<Guid> AddRoomAsync(AddRoomViewModel room, Guid userId);
        Task<IEnumerable<RoomListViewModel>> GetAllRoomsAsync();
        Task<EditRoomViewModel?> GetRoomForEditAsync(Guid roomId);
        Task<bool> UpdateRoomAsync(EditRoomViewModel model);
        Task<bool> DeactivateRoomAsync(Guid roomId);
        Task<Room> GetSingleRoomById(Guid roomId);
    }
}
