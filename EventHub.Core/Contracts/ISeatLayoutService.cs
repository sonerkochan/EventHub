using EventHub.Core.Models.Room;

namespace EventHub.Core.Contracts
{
    public interface ISeatLayoutService
    {
        Task<SeatLayoutEditorViewModel> GetLayoutEditorDataAsync(Guid roomId);
        Task SaveLayoutAsync(SaveSeatLayoutRequest request, Guid userId);
        Task<ZoneDto> CreateZoneAsync(CreateZoneRequest request, Guid userId);
        Task AssignSeatsToZoneAsync(AssignZoneRequest request);
        Task RemoveSeatsFromZoneAsync(RemoveFromZoneRequest request);
        Task DeleteZoneAsync(Guid zoneId);
    }
}
