using EventHub.Core.Contracts;
using EventHub.Core.Models.Currency;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace EventHub.Core.Services
{
    public class CurrencyDisplayService : ICurrencyDisplayService
    {
        private readonly ICurrencyExchangeRateService exchangeRateService;
        private readonly ILogger<CurrencyDisplayService> logger;

        public CurrencyDisplayService(
            ICurrencyExchangeRateService _exchangeRateService,
            ILogger<CurrencyDisplayService> _logger)
        {
            exchangeRateService = _exchangeRateService;
            logger = _logger;
        }

        public async Task<CurrencyDisplayValue> FormatAsync(decimal amount, string? sourceCurrency = null)
        {
            var from = NormalizeCurrency(sourceCurrency ?? exchangeRateService.BaseCurrency);
            var to = NormalizeCurrency(exchangeRateService.DisplayCurrency);

            try
            {
                var converted = from == to
                    ? Round(amount)
                    : await exchangeRateService.ConvertAsync(amount, from, to);

                return CreateValue(converted, to);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Unable to convert display currency from {SourceCurrency} to {DisplayCurrency}. Falling back to source currency.",
                    from,
                    to);

                return CreateValue(Round(amount), from);
            }
        }

        private static CurrencyDisplayValue CreateValue(decimal amount, string currency)
        {
            return new CurrencyDisplayValue
            {
                Amount = amount,
                Currency = currency,
                Text = $"{amount.ToString("0.00", CultureInfo.InvariantCulture)} {currency}"
            };
        }

        private static decimal Round(decimal amount)
            => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

        private static string NormalizeCurrency(string? currency)
            => string.IsNullOrWhiteSpace(currency)
                ? "EUR"
                : currency.Trim().ToUpperInvariant();
    }
}
