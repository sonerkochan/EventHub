using EventHub.Core.Contracts;
using EventHub.Core.Models.Zone;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventHub.Core.Services
{
    public class ZoneService : IZoneService
    {
        private readonly IRepository repo;

        public ZoneService(IRepository _repo)
        {
            repo = _repo;
        }

        public async Task<Guid> CreateAsync(CreateZoneViewModel model, Guid createdBy)
        {
            var entity = new Zone
            {
                Id = Guid.NewGuid(),
                RoomId = model.RoomId,
                CreatedBy = createdBy,
                Name = model.Name,
                ZoneType = model.ZoneType,
                Capacity = model.Capacity,
                DisplayOrder = model.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<IEnumerable<ZoneListViewModel>> GetByRoomAsync(Guid roomId)
        {
            return await repo.AllReadonly<Zone>()
                .Where(z => z.RoomId == roomId && z.IsActive)
                .Join(
                    repo.AllReadonly<Room>(),
                    z => z.RoomId,
                    r => r.RoomId,
                    (z, r) => new ZoneListViewModel
                    {
                        Id = z.Id,
                        RoomId = z.RoomId,
                        RoomName = r.Name,
                        Name = z.Name,
                        ZoneType = z.ZoneType,
                        Capacity = z.Capacity,
                        DisplayOrder = z.DisplayOrder,
                        IsActive = z.IsActive
                    })
                .OrderBy(z => z.DisplayOrder)
                .ToListAsync();
        }

        public async Task<EditZoneViewModel?> GetForEditAsync(Guid id)
        {
            var entity = await repo.GetByIdAsync<Zone>(id);
            if (entity == null) return null;

            return new EditZoneViewModel
            {
                Id = entity.Id,
                RoomId = entity.RoomId,
                Name = entity.Name!,
                ZoneType = entity.ZoneType,
                Capacity = entity.Capacity,
                DisplayOrder = entity.DisplayOrder
            };
        }

        public async Task<bool> UpdateAsync(EditZoneViewModel model)
        {
            var entity = await repo.GetByIdAsync<Zone>(model.Id);
            if (entity == null) return false;

            entity.Name = model.Name;
            entity.ZoneType = model.ZoneType;
            entity.Capacity = model.Capacity;
            entity.DisplayOrder = model.DisplayOrder;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            var entity = await repo.GetByIdAsync<Zone>(id);
            if (entity == null) return false;

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateAsync(Guid id)
        {
            var entity = await repo.GetByIdAsync<Zone>(id);
            if (entity == null) return false;

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }
    }
}
