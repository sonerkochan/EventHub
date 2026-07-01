using EventHub.Core.Contracts;
using EventHub.Core.Models.Payment;
using Microsoft.Extensions.Options;
using Stripe;

namespace EventHub.Core.Services
{
    public class StripeRefundProcessor : IPaymentRefundProcessor
    {
        private readonly StripeOptions options;

        public StripeRefundProcessor(IOptions<StripeOptions> options)
        {
            this.options = options.Value;
            StripeConfiguration.ApiKey = this.options.SecretKey;
        }

        public async Task<PaymentRefundResult> RefundPaymentIntentAsync(PaymentRefundRequest request)
        {
            try
            {
                var service = new Stripe.RefundService();
                var refund = await service.CreateAsync(new RefundCreateOptions
                {
                    PaymentIntent = request.PaymentIntentId,
                    Amount = request.AmountMinor,
                    Metadata = request.Metadata
                });

                return PaymentRefundResult.Succeeded(refund.Id);
            }
            catch (StripeException ex)
            {
                return PaymentRefundResult.Failed(ex.Message);
            }
        }
    }
}
