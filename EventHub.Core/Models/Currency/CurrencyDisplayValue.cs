namespace EventHub.Core.Models.Currency
{
    public class CurrencyDisplayValue
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public string Text { get; set; } = "0.00 EUR";
    }
}
