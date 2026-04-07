using EventHub.Core.Contracts;
using EventHub.Core.Models.Room;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Core.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRepository _repo;

        public RoomService(IRepository repo)
        {
            _repo = repo;
        }

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
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            return entity.RoomId;
        }

        public async Task<IEnumerable<RoomListViewModel>> GetAllRoomsAsync()
        {
            return await _repo.AllReadonly<Room>()
                .Where(r => r.IsActive)
                .Join(
                    _repo.AllReadonly<Venue>(),
                    r => r.VenueId,
                    v => v.Id,
                    (r, v) => new RoomListViewModel
                    {
                        Id = r.RoomId,
                        Name = r.Name,
                        VenueId = r.VenueId,
                        VenueName = v.Name,
                        Description = r.Description,
                        Capacity = r.Capacity,
                        RoomType = r.RoomType,
                        IsActive = r.IsActive
                    })
                .ToListAsync();
        }

        public async Task<EditRoomViewModel?> GetRoomForEditAsync(Guid roomId)
        {
            var entity = await _repo.GetByIdAsync<Room>(roomId);
            if (entity == null) return null;

            return new EditRoomViewModel
            {
                Id = entity.RoomId,
                VenueId = entity.VenueId,
                Name = entity.Name!,
                Description = entity.Description,
                Capacity = entity.Capacity,
                RoomType = entity.RoomType
            };
        }

        public async Task<bool> UpdateRoomAsync(EditRoomViewModel model)
        {
            var entity = await _repo.GetByIdAsync<Room>(model.Id);
            if (entity == null) return false;

            entity.VenueId = model.VenueId;
            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.Capacity = model.Capacity;
            entity.RoomType = model.RoomType;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateRoomAsync(Guid roomId)
        {
            var entity = await _repo.GetByIdAsync<Room>(roomId);
            if (entity == null) return false;

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<Room> GetSingleRoomById(Guid roomId)
        {
            var entity = await _repo.GetByIdAsync<Room>(roomId);
            if (entity is null)
            {
                throw new Exception($"Room with ID {roomId} does not exist.");
            }
            return entity;
        }
    }
}
