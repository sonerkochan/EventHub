namespace EventHub.Core.Models.Travelis
{
    public class TravelisOptions
    {
        public const string Section = "Travelis";

        public string ApiBaseUrl { get; set; } = "https://travelis-api.sadulov.com";

        public string PartnerBaseUrl { get; set; } = "https://travelis.sadulov.com/";

        public string HotelImageBaseUrl { get; set; } = "https://travelis.sadulov.com/uploads/hotels/";

        public int CacheMinutes { get; set; } = 15;

        public int TimeoutSeconds { get; set; } = 4;
    }
}
