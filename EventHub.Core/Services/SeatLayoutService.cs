using EventHub.Core.Contracts;
using EventHub.Core.Models.Room;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EventHub.Core.Services
{
    public class SeatLayoutService : ISeatLayoutService
    {
        private readonly IRepository _repo;

        public SeatLayoutService(IRepository repo)
        {
            _repo = repo;
        }

        public async Task<SeatLayoutEditorViewModel> GetLayoutEditorDataAsync(Guid roomId)
        {
            var room = await _repo.GetByIdAsync<Room>(roomId)
                ?? throw new Exception($"Room {roomId} not found.");

            var layout = await _repo.AllReadonly<SeatLayout>()
                .Where(sl => sl.RoomId == roomId && sl.IsActive)
                .FirstOrDefaultAsync();

            var seats = await _repo.AllReadonly<Seat>()
                .Where(s => s.RoomId == roomId && s.IsActive)
                .Select(s => new SeatDto
                {
                    Id = s.Id,
                    Row = s.Row,
                    Column = s.Column,
                    SeatNumber = s.SeatNumber,
                    ZoneId = s.ZoneId,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            var zones = await _repo.AllReadonly<Zone>()
                .Where(z => z.RoomId == roomId && z.IsActive)
                .Select(z => new ZoneDto
                {
                    Id = z.Id,
                    Name = z.Name!,
                    ZoneType = z.ZoneType,
                    SeatCount = _repo.AllReadonly<Seat>()
                        .Count(s => s.ZoneId == z.Id && s.IsActive)
                })
                .ToListAsync();

            int gridRows = 10;
            int gridColumns = 10;

            if (layout?.Structure != null)
            {
                try
                {
                    var structure = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(layout.Structure);
                    if (structure != null)
                    {
                        if (structure.TryGetValue("gridRows", out var gr)) gridRows = gr.GetInt32();
                        if (structure.TryGetValue("gridColumns", out var gc)) gridColumns = gc.GetInt32();
                    }
                }
                catch { }
            }

            if (seats.Any())
            {
                gridRows = Math.Max(gridRows, seats.Max(s => s.Row) + 1);
                gridColumns = Math.Max(gridColumns, seats.Max(s => s.Column) + 1);
            }

            return new SeatLayoutEditorViewModel
            {
                RoomId = room.RoomId,
                RoomName = room.Name ?? "Unnamed Room",
                RoomCapacity = room.Capacity,
                LayoutId = layout?.Id,
                LayoutName = layout?.Name,
                GridRows = gridRows,
                GridColumns = gridColumns,
                StructureJson = layout?.Structure,
                Seats = seats,
                Zones = zones
            };
        }

        public async Task SaveLayoutAsync(SaveSeatLayoutRequest request, Guid userId)
        {
            var existingSeats = await _repo.All<Seat>()
                .Where(s => s.RoomId == request.RoomId && s.IsActive)
                .ToListAsync();

            var existingByPos = existingSeats
                .ToDictionary(s => (s.Row, s.Column));

            var incomingPositions = new HashSet<(int Row, int Column)>();
            int seatNumber = 1;

            var sortedIncoming = request.Seats
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Column)
                .ToList();

            foreach (var dto in sortedIncoming)
            {
                var pos = (dto.Row, dto.Column);
                incomingPositions.Add(pos);

                if (existingByPos.TryGetValue(pos, out var existing))
                {
                    existing.SeatNumber = seatNumber;
                    existing.PositionX = dto.Column;
                    existing.PositionY = dto.Row;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var newSeat = new Seat
                    {
                        Id = Guid.NewGuid(),
                        RoomId = request.RoomId,
                        ZoneId = null,
                        SeatNumber = seatNumber,
                        Row = dto.Row,
                        Column = dto.Column,
                        PositionX = dto.Column,
                        PositionY = dto.Row,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _repo.AddAsync(newSeat);
                }

                seatNumber++;
            }

            foreach (var existing in existingSeats)
            {
                var pos = (existing.Row, existing.Column);
                if (!incomingPositions.Contains(pos))
                {
                    existing.IsActive = false;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }

            var layout = await _repo.All<SeatLayout>()
                .Where(sl => sl.RoomId == request.RoomId && sl.IsActive)
                .FirstOrDefaultAsync();

            var structureJson = JsonSerializer.Serialize(new
            {
                gridRows = request.GridRows,
                gridColumns = request.GridColumns
            });

            if (layout != null)
            {
                layout.Name = request.LayoutName ?? layout.Name;
                layout.Structure = structureJson;
                layout.TotalSeats = sortedIncoming.Count;
                layout.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                layout = new SeatLayout
                {
                    Id = Guid.NewGuid(),
                    RoomId = request.RoomId,
                    CreatedBy = userId,
                    Name = request.LayoutName ?? "Default Layout",
                    Structure = structureJson,
                    TotalSeats = sortedIncoming.Count,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _repo.AddAsync(layout);
            }

            await _repo.SaveChangesAsync();
        }

        public async Task<ZoneDto> CreateZoneAsync(CreateZoneRequest request, Guid userId)
        {
            var zone = new Zone
            {
                Id = Guid.NewGuid(),
                RoomId = request.RoomId,
                CreatedBy = userId,
                Name = request.Name,
                ZoneType = request.ZoneType,
                Capacity = 0,
                DisplayOrder = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(zone);
            await _repo.SaveChangesAsync();

            return new ZoneDto
            {
                Id = zone.Id,
                Name = zone.Name,
                ZoneType = zone.ZoneType,
                SeatCount = 0
            };
        }

        public async Task AssignSeatsToZoneAsync(AssignZoneRequest request)
        {
            var seats = await _repo.All<Seat>()
                .Where(s => request.SeatIds.Contains(s.Id) && s.RoomId == request.RoomId)
                .ToListAsync();

            foreach (var seat in seats)
            {
                seat.ZoneId = request.ZoneId;
                seat.UpdatedAt = DateTime.UtcNow;
            }

            var zone = await _repo.GetByIdAsync<Zone>(request.ZoneId);
            if (zone != null)
            {
                zone.Capacity = await _repo.AllReadonly<Seat>()
                    .CountAsync(s => s.ZoneId == zone.Id && s.IsActive);
            }

            await _repo.SaveChangesAsync();
        }

        public async Task RemoveSeatsFromZoneAsync(RemoveFromZoneRequest request)
        {
            var seats = await _repo.All<Seat>()
                .Where(s => request.SeatIds.Contains(s.Id) && s.RoomId == request.RoomId)
                .ToListAsync();

            var affectedZoneIds = seats
                .Where(s => s.ZoneId.HasValue)
                .Select(s => s.ZoneId!.Value)
                .Distinct()
                .ToList();

            foreach (var seat in seats)
            {
                seat.ZoneId = null;
                seat.UpdatedAt = DateTime.UtcNow;
            }

            foreach (var zoneId in affectedZoneIds)
            {
                var zone = await _repo.GetByIdAsync<Zone>(zoneId);
                if (zone != null)
                {
                    zone.Capacity = await _repo.AllReadonly<Seat>()
                        .CountAsync(s => s.ZoneId == zone.Id && s.IsActive && !request.SeatIds.Contains(s.Id));
                }
            }

            await _repo.SaveChangesAsync();
        }

        public async Task DeleteZoneAsync(Guid zoneId)
        {
            var zone = await _repo.GetByIdAsync<Zone>(zoneId);
            if (zone == null) return;

            var seats = await _repo.All<Seat>()
                .Where(s => s.ZoneId == zoneId)
                .ToListAsync();

            foreach (var seat in seats)
            {
                seat.ZoneId = null;
                seat.UpdatedAt = DateTime.UtcNow;
            }

            zone.IsActive = false;
            zone.UpdatedAt = DateTime.UtcNow;

            await _repo.SaveChangesAsync();
        }
    }
}
