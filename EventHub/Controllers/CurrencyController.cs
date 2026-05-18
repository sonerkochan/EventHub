using EventHub.Core.Contracts;
using EventHub.Core.Models.Currency;
using EventHub.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EventHub.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/currency")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyExchangeRateService exchangeRateService;
        private readonly CurrencyOptions options;

        public CurrencyController(
            ICurrencyExchangeRateService _exchangeRateService,
            IOptions<CurrencyOptions> _options)
        {
            exchangeRateService = _exchangeRateService;
            options = _options.Value;
        }

        [HttpGet("rate")]
        public async Task<ActionResult<CurrencyRateResponse>> Rate(string? to, string? from = null)
        {
            var baseCurrency = NormalizeCurrency(from ?? options.BaseCurrency);
            var requestedCurrency = NormalizeCurrency(to ?? options.DisplayCurrency);
            var supportedCurrencies = GetSupportedCurrencies();
            var currency = supportedCurrencies.Contains(requestedCurrency)
                ? requestedCurrency
                : NormalizeCurrency(options.DisplayCurrency);

            var responseCurrency = currency;
            var rate = 1m;

            try
            {
                rate = baseCurrency == currency
                    ? 1m
                    : await exchangeRateService.GetRateAsync(baseCurrency, currency);
            }
            catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException or TaskCanceledException)
            {
                responseCurrency = "EUR";
                rate = 1m;
            }

            return Ok(new CurrencyRateResponse
            {
                BaseCurrency = baseCurrency,
                Currency = responseCurrency,
                Rate = rate,
                SupportedCurrencies = supportedCurrencies
            });
        }

        [HttpGet("supported")]
        public ActionResult<string[]> Supported()
            => Ok(GetSupportedCurrencies());

        private string[] GetSupportedCurrencies()
            => (options.SupportedCurrencies == null || options.SupportedCurrencies.Length == 0
                    ? ["EUR", "USD", "JPY", "GBP", "AUD", "CAD", "CHF", "CNY", "SEK", "NZD", "TRY"]
                    : options.SupportedCurrencies)
                .Select(NormalizeCurrency)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

        private static string NormalizeCurrency(string? currency)
            => string.IsNullOrWhiteSpace(currency)
                ? "EUR"
                : currency.Trim().ToUpperInvariant();
    }
}
