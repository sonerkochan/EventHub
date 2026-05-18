using EventHub.Core.Contracts;
using EventHub.Core.Models.Currency;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EventHub.Core.Services
{
    public class FrankfurterCurrencyExchangeRateService : ICurrencyExchangeRateService
    {
        private static readonly Dictionary<string, (DateOnly Date, decimal Rate)> RatesCache = new();

        private readonly HttpClient httpClient;
        private readonly CurrencyOptions options;

        public FrankfurterCurrencyExchangeRateService(
            HttpClient _httpClient,
            IOptions<CurrencyOptions> _options)
        {
            httpClient = _httpClient;
            options = _options.Value;
        }

        public string BaseCurrency => NormalizeCurrency(options.BaseCurrency);

        public string DisplayCurrency => NormalizeCurrency(options.DisplayCurrency);

        public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            var rate = await GetRateAsync(fromCurrency, toCurrency);
            return Math.Round(amount * rate, 2, MidpointRounding.AwayFromZero);
        }

        public async Task<decimal> GetRateAsync(string fromCurrency, string toCurrency)
        {
            var from = NormalizeCurrency(fromCurrency);
            var to = NormalizeCurrency(toCurrency);

            if (from == to)
            {
                return 1m;
            }

            var cacheKey = $"{from}:{to}";
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (RatesCache.TryGetValue(cacheKey, out var cached) && cached.Date == today)
            {
                return cached.Rate;
            }

            var url = BuildRateUrl(options.ExchangeRatesApiBaseUrl, from, to);

            try
            {
                using var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException(
                        $"Unable to fetch exchange rate from {from} to {to}. Status: {(int)response.StatusCode}. Response: {body}");
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var document = await JsonDocument.ParseAsync(stream);

                if (!TryReadRate(document.RootElement, to, out var rate))
                {
                    throw new InvalidOperationException($"Unable to fetch exchange rate from {from} to {to}.");
                }

                RatesCache[cacheKey] = (today, rate);
                return rate;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Unable to parse exchange rate from {from} to {to}.", ex);
            }
        }

        private static string NormalizeCurrency(string? currency)
            => string.IsNullOrWhiteSpace(currency)
                ? "EUR"
                : currency.Trim().ToUpperInvariant();

        private static string BuildRateUrl(string? configuredUrl, string from, string to)
        {
            var baseUrl = string.IsNullOrWhiteSpace(configuredUrl)
                ? "https://api.frankfurter.dev/v2/rate"
                : configuredUrl.Trim().TrimEnd('/');

            if (baseUrl.EndsWith("/rate", StringComparison.OrdinalIgnoreCase))
            {
                return $"{baseUrl}/{Uri.EscapeDataString(from)}/{Uri.EscapeDataString(to)}";
            }

            return $"{baseUrl}?base={Uri.EscapeDataString(from)}&quotes={Uri.EscapeDataString(to)}";
        }

        private static bool TryReadRate(JsonElement root, string to, out decimal rate)
        {
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    if (TryReadArrayRateItem(item, to, out rate))
                    {
                        return true;
                    }
                }

                rate = default;
                return false;
            }

            if (root.TryGetProperty("rate", out var singleRate)
                && singleRate.TryGetDecimal(out rate))
            {
                return true;
            }

            if (root.TryGetProperty("rates", out var rates)
                && rates.ValueKind == JsonValueKind.Object
                && rates.TryGetProperty(to, out var quotedRate)
                && quotedRate.TryGetDecimal(out rate))
            {
                return true;
            }

            rate = default;
            return false;
        }

        private static bool TryReadArrayRateItem(JsonElement item, string to, out decimal rate)
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                rate = default;
                return false;
            }

            var quoteMatches = item.TryGetProperty("quote", out var quote)
                && string.Equals(quote.GetString(), to, StringComparison.OrdinalIgnoreCase);

            if (quoteMatches
                && item.TryGetProperty("rate", out var itemRate)
                && itemRate.TryGetDecimal(out rate))
            {
                return true;
            }

            rate = default;
            return false;
        }
    }
}
