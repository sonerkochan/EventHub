using EventHub.Core.Contracts;
using EventHub.Core.Models.Room;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EventHub.Core.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRepository _repo;
        public RoomService(IRepository repo)
        {
            _repo = repo;
        }
        [Description("Creates a new Room and adds it to the database.")]
        public async Task<Guid> AddRoomAsync(AddRoomViewModel room, Guid userId)
        {
            var entity = new Room()
            {
                RoomId = Guid.NewGuid(),
                VenueId = room.VenueId,
                CreatedBy = userId,
                Name = room.Name,
                Description = room.Description,
                Capacity = room.Capacity,
                RoomType = room.RoomType,
                IsActive = room.IsActive
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            return entity.RoomId;
        }

        public async Task<Room> GetSingleRoomById(Guid roomId)
        {
            var roomExists = await _repo.GetByIdAsync<Room>(roomId);
            if (roomExists is null)
            {
                throw new Exception($"Room with ID {roomId} does not exist.");
            }
            return roomExists;
        }

        public async Task DeleteRoomAsync(Room room)
        {
            var roomExists = await _repo.GetByIdAsync<Room>(room.RoomId);
            if (roomExists is null)
            {
                throw new Exception($"Room with ID {room.RoomId} does not exist.");
            }
            await _repo.DeleteAsync<Room>(room);
        }

        public Task<IEnumerable<RoomListViewModel>> GetAllRoomsAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateRoomAsync(Room room)
        {
            throw new NotImplementedException();
        }
    }
}
