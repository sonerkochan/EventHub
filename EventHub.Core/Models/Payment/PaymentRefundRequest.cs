using System;
using System.Collections.Generic;

namespace EventHub.Core.Models.Payment
{
    public class PaymentRefundRequest
    {
        public string PaymentIntentId { get; set; } = null!;
        public long AmountMinor { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = [];
    }
}
