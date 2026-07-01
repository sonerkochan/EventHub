using System.Collections.Concurrent;
using System.Text.Json;
using EventHub.Core.Contracts;
using EventHub.Core.Models.Travelis;
using Microsoft.Extensions.Options;

namespace EventHub.Core.Services
{
    public class TravelisHotelService : ITravelisHotelService
    {
        private static readonly ConcurrentDictionary<string, CacheEntry> Cache =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly HttpClient httpClient;
        private readonly TravelisOptions options;

        public TravelisHotelService(
            HttpClient _httpClient,
            IOptions<TravelisOptions> _options)
        {
            httpClient = _httpClient;
            options = _options.Value;
        }

        public async Task<IReadOnlyList<TravelisHotelViewModel>> GetHotelsByCityAsync(
            string city,
            CancellationToken cancellationToken = default)
        {
            var normalizedCity = NormalizeCity(city);
            if (string.IsNullOrWhiteSpace(normalizedCity))
            {
                return Array.Empty<TravelisHotelViewModel>();
            }

            if (Cache.TryGetValue(normalizedCity, out var cached)
                && cached.ExpiresAt > DateTimeOffset.UtcNow)
            {
                return cached.Hotels;
            }

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds)));

            try
            {
                var requestUri = BuildCityUrl(normalizedCity);
                using var response = await httpClient.GetAsync(requestUri, timeoutSource.Token);
                if (!response.IsSuccessStatusCode)
                {
                    return Array.Empty<TravelisHotelViewModel>();
                }

                await using var stream = await response.Content.ReadAsStreamAsync(timeoutSource.Token);
                var hotels = await JsonSerializer.DeserializeAsync<List<TravelisHotelResponse>>(
                    stream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                    timeoutSource.Token);

                var mappedHotels = (hotels ?? [])
                    .Where(IsDisplayable)
                    .Select(MapHotel)
                    .Where(hotel => !string.IsNullOrWhiteSpace(hotel.Name))
                    .ToList();

                Cache[normalizedCity] = new CacheEntry(
                    mappedHotels,
                    DateTimeOffset.UtcNow.AddMinutes(Math.Max(1, options.CacheMinutes)));

                return mappedHotels;
            }
            catch (OperationCanceledException)
            {
                return Array.Empty<TravelisHotelViewModel>();
            }
            catch (HttpRequestException)
            {
                return Array.Empty<TravelisHotelViewModel>();
            }
            catch (JsonException)
            {
                return Array.Empty<TravelisHotelViewModel>();
            }
        }

        private string BuildCityUrl(string city)
        {
            var apiBaseUrl = NormalizeBaseUrl(options.ApiBaseUrl, "https://travelis-api.sadulov.com");
            return $"{apiBaseUrl}/hotel/city/{Uri.EscapeDataString(city)}";
        }

        private TravelisHotelViewModel MapHotel(TravelisHotelResponse hotel)
        {
            var imageBaseUrl = NormalizeBaseUrl(
                options.HotelImageBaseUrl,
                "https://travelis.sadulov.com/uploads/hotels");

            return new TravelisHotelViewModel
            {
                Id = hotel.Id,
                Name = hotel.Name?.Trim() ?? string.Empty,
                Country = hotel.Country,
                City = hotel.City,
                Street = hotel.Street,
                PostalCode = hotel.PostalCode,
                PhoneNumber = hotel.PhoneNumber,
                Email = hotel.Email,
                Images = (hotel.Images ?? [])
                    .Where(image => !string.IsNullOrWhiteSpace(image.Name))
                    .Select(image => new TravelisHotelImageViewModel
                    {
                        Id = image.Id,
                        Name = image.Name!.Trim(),
                        Url = $"{imageBaseUrl}/{Uri.EscapeDataString(image.Name.Trim())}"
                    })
                    .ToList()
            };
        }

        private static bool IsDisplayable(TravelisHotelResponse hotel)
        {
            if (hotel.Approved.HasValue && hotel.Approved.Value == false)
            {
                return false;
            }

            if (hotel.Status.HasValue && hotel.Status.Value != 1)
            {
                return false;
            }

            return true;
        }

        private static string NormalizeCity(string? city)
            => string.IsNullOrWhiteSpace(city) ? string.Empty : city.Trim();

        private static string NormalizeBaseUrl(string? configuredUrl, string fallback)
            => string.IsNullOrWhiteSpace(configuredUrl)
                ? fallback.TrimEnd('/')
                : configuredUrl.Trim().TrimEnd('/');

        private sealed record CacheEntry(
            IReadOnlyList<TravelisHotelViewModel> Hotels,
            DateTimeOffset ExpiresAt);

        private sealed class TravelisHotelResponse
        {
            public string? Id { get; set; }

            public string? Name { get; set; }

            public string? Country { get; set; }

            public string? City { get; set; }

            public string? Street { get; set; }

            public string? PostalCode { get; set; }

            public string? PhoneNumber { get; set; }

            public string? Email { get; set; }

            public int? Status { get; set; }

            public bool? Approved { get; set; }

            public List<TravelisHotelImageResponse>? Images { get; set; }
        }

        private sealed class TravelisHotelImageResponse
        {
            public string? Id { get; set; }

            public string? Name { get; set; }
        }
    }
}
