using EventHub.Core.Models.Payment;

namespace EventHub.Core.Contracts
{
    public interface IPaymentRefundProcessor
    {
        Task<PaymentRefundResult> RefundPaymentIntentAsync(PaymentRefundRequest request);
    }
}
