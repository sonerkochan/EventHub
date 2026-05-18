namespace EventHub.Core.Models.Currency
{
    public class CurrencyOptions
    {
        public const string Section = "Currency";

        public string BaseCurrency { get; set; } = "EUR";
        public string DisplayCurrency { get; set; } = "EUR";
        public string ExchangeRatesApiBaseUrl { get; set; } = "https://api.frankfurter.dev/v2/rate";
        public string[] SupportedCurrencies { get; set; } = new[] { "EUR", "USD", "JPY", "GBP", "AUD", "CAD", "CHF", "CNY", "SEK", "NZD", "TRY" };
    }
}
