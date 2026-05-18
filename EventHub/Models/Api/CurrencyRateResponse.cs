namespace EventHub.Models.Api
{
    public class CurrencyRateResponse
    {
        public string BaseCurrency { get; set; } = "EUR";
        public string Currency { get; set; } = "EUR";
        public decimal Rate { get; set; } = 1m;
        public string[] SupportedCurrencies { get; set; } = [];
    }
}
