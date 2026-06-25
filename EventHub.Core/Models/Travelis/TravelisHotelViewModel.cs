namespace EventHub.Core.Models.Travelis
{
    public class TravelisHotelViewModel
    {
        public string? Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Country { get; set; }

        public string? City { get; set; }

        public string? Street { get; set; }

        public string? PostalCode { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public IReadOnlyList<TravelisHotelImageViewModel> Images { get; set; } =
            Array.Empty<TravelisHotelImageViewModel>();

        public string DisplayAddress
        {
            get
            {
                var parts = new[] { Street, PostalCode, City, Country }
                    .Where(part => !string.IsNullOrWhiteSpace(part));

                return string.Join(", ", parts);
            }
        }
    }
}
