using EventHub.Core.Models.Travelis;

namespace EventHub.Core.Contracts
{
    public interface ITravelisHotelService
    {
        Task<IReadOnlyList<TravelisHotelViewModel>> GetHotelsByCityAsync(
            string city,
            CancellationToken cancellationToken = default);
    }
}
