using EventHub.Core.Models.Payment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventHub.Core.Contracts
{
    public interface IPaymentService
    {
        /// <summary>
        /// Creates a Stripe Checkout Session and saves a pending Payment record.
        /// Returns the Stripe-hosted checkout URL to redirect the user to.
        /// </summary>
        Task<string> CreateCheckoutSessionAsync(CreateCheckoutRequest request);

        /// <summary>
        /// Creates a Stripe Checkout Session for a cart of already-reserved seat tickets.
        /// Each seat is its own LineItem so receipts show per-zone breakdown.
        /// Webhook flips the listed ticket IDs from Reserved to Purchased on payment success.
        /// </summary>
        Task<string> CreateSeatCheckoutSessionAsync(CreateSeatCheckoutRequest request);

        /// <summary>
        /// Processes an incoming Stripe webhook payload.
        /// On checkout.session.completed, creates tickets and marks the payment as accepted.
        /// </summary>
        Task HandleWebhookAsync(string payload, string stripeSignature);

        /// <summary>
        /// Returns the payment history for a given user.
        /// </summary>
        Task<IEnumerable<PaymentListViewModel>> GetPaymentHistoryAsync(Guid userId);
    }
}