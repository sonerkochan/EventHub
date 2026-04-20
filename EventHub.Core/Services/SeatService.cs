using EventHub.Core.Contracts;
using EventHub.Core.Models.Seat;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventHub.Core.Services
{
    public class SeatService : ISeatService
    {
        private readonly IRepository repo;

        public SeatService(IRepository _repo)
        {
            repo = _repo;
        }

        public async Task<Guid> CreateAsync(CreateSeatViewModel model)
        {
            var entity = new Seat
            {
                Id = Guid.NewGuid(),
                RoomId = model.RoomId,
                ZoneId = model.ZoneId,
                SeatNumber = model.SeatNumber,
                Row = model.Row,
                Column = model.Column,
                PositionX = model.PositionX,
                PositionY = model.PositionY,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<IEnumerable<Guid>> CreateBatchAsync(Guid roomId, int count, Guid? zoneId)
        {
            var lastSeat = await repo.AllReadonly<Seat>()
                .Where(s => s.RoomId == roomId)
                .OrderByDescending(s => s.SeatNumber)
                .FirstOrDefaultAsync();

            int nextNumber = (lastSeat?.SeatNumber ?? 0) + 1;
            var ids = new List<Guid>();
            var seats = new List<Seat>();

            for (int i = 0; i < count; i++)
            {
                var seat = new Seat
                {
                    Id = Guid.NewGuid(),
                    RoomId = roomId,
                    ZoneId = zoneId,
                    SeatNumber = nextNumber + i,
                    Row = 1,
                    Column = nextNumber + i,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                seats.Add(seat);
                ids.Add(seat.Id);
            }

            await repo.AddRangeAsync(seats);
            await repo.SaveChangesAsync();
            return ids;
        }

        public async Task<IEnumerable<SeatListViewModel>> GetByRoomAsync(Guid roomId)
        {
            return await repo.AllReadonly<Seat>()
                .Where(s => s.RoomId == roomId && s.IsActive)
                .GroupJoin(
                    repo.AllReadonly<Zone>(),
                    s => s.ZoneId,
                    z => z.Id,
                    (s, zones) => new { s, zones })
                .SelectMany(
                    x => x.zones.DefaultIfEmpty(),
                    (x, z) => new SeatListViewModel
                    {
                        Id = x.s.Id,
                        RoomId = x.s.RoomId,
                        ZoneId = x.s.ZoneId,
                        ZoneName = z != null ? z.Name : null,
                        SeatNumber = x.s.SeatNumber,
                        Row = x.s.Row,
                        Column = x.s.Column,
                        IsActive = x.s.IsActive
                    })
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Column)
                .ToListAsync();
        }

        public async Task<EditSeatViewModel?> GetForEditAsync(Guid id)
        {
            var entity = await repo.GetByIdAsync<Seat>(id);
            if (entity == null) return null;

            return new EditSeatViewModel
            {
                Id = entity.Id,
                RoomId = entity.RoomId,
                ZoneId = entity.ZoneId,
                SeatNumber = entity.SeatNumber,
                Row = entity.Row,
                Column = entity.Column,
                PositionX = entity.PositionX,
                PositionY = entity.PositionY
            };
        }

        public async Task<bool> UpdateAsync(EditSeatViewModel model)
        {
            var entity = await repo.GetByIdAsync<Seat>(model.Id);
            if (entity == null) return false;

            entity.RoomId = model.RoomId;
            entity.ZoneId = model.ZoneId;
            entity.SeatNumber = model.SeatNumber;
            entity.Row = model.Row;
            entity.Column = model.Column;
            entity.PositionX = model.PositionX;
            entity.PositionY = model.PositionY;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            var entity = await repo.GetByIdAsync<Seat>(id);
            if (entity == null) return false;

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateAsync(Guid id)
        {
            var entity = await repo.GetByIdAsync<Seat>(id);
            if (entity == null) return false;

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignToZoneAsync(Guid seatId, Guid zoneId)
        {
            var entity = await repo.GetByIdAsync<Seat>(seatId);
            if (entity == null) return false;

            entity.ZoneId = zoneId;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromZoneAsync(Guid seatId)
        {
            var entity = await repo.GetByIdAsync<Seat>(seatId);
            if (entity == null) return false;

            entity.ZoneId = null;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }
    }
}
