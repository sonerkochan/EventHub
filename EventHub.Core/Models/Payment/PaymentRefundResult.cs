namespace EventHub.Core.Models.Payment
{
    public class PaymentRefundResult
    {
        public bool Success { get; set; }
        public string? StripeRefundId { get; set; }
        public string? ErrorMessage { get; set; }

        public static PaymentRefundResult Failed(string errorMessage)
            => new()
            {
                Success = false,
                ErrorMessage = errorMessage
            };

        public static PaymentRefundResult Succeeded(string stripeRefundId)
            => new()
            {
                Success = true,
                StripeRefundId = stripeRefundId
            };
    }
}
