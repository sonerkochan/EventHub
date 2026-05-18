namespace EventHub.Core.Services
{
    public static class EventDisplayPriceCalculator
    {
        public static decimal? GetLowestPaidPrice(
            decimal basePrice,
            IEnumerable<decimal> tierPrices,
            IEnumerable<decimal>? ticketPrices = null)
        {
            var paidPrices = tierPrices.Where(price => price > 0).ToList();

            if (basePrice > 0)
            {
                paidPrices.Add(basePrice);
            }

            if (paidPrices.Count == 0 && ticketPrices != null)
            {
                paidPrices.AddRange(ticketPrices.Where(price => price > 0));
            }

            return paidPrices.Count == 0 ? null : paidPrices.Min();
        }
    }
}
