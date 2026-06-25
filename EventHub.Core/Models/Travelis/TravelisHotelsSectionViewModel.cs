namespace EventHub.Core.Models.Travelis
{
    public class TravelisHotelsSectionViewModel
    {
        public string? City { get; set; }

        public string PartnerBaseUrl { get; set; } = "https://travelis.sadulov.com/";

        public IReadOnlyList<TravelisHotelViewModel> Hotels { get; set; } =
            Array.Empty<TravelisHotelViewModel>();

        public bool IsUnavailable { get; set; }

        public bool IsMissingCity => string.IsNullOrWhiteSpace(City);
    }
}
