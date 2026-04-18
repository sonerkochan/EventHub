namespace EventHub.Core.Models.Payment
{
    public class StripeOptions
    {
        public const string Section = "Stripe";
        public string SecretKey { get; set; } = null!;
        public string PublishableKey { get; set; } = null!;
        public string WebhookSecret { get; set; } = null!;
    }
}