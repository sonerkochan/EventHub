using EventHub.Core.Models.Currency;

namespace EventHub.Core.Contracts
{
    public interface ICurrencyDisplayService
    {
        Task<CurrencyDisplayValue> FormatAsync(decimal amount, string? sourceCurrency = null);
    }
}
