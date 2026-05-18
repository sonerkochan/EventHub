namespace EventHub.Core.Contracts
{
    public interface ICurrencyExchangeRateService
    {
        string BaseCurrency { get; }
        string DisplayCurrency { get; }
        Task<decimal> GetRateAsync(string fromCurrency, string toCurrency);
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
    }
}
