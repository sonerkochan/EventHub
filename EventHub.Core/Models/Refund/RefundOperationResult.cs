using System;

namespace EventHub.Core.Models.Refund
{
    public class RefundOperationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid? RefundId { get; set; }
        public float RefundAmount { get; set; }

        public static RefundOperationResult Failed(string errorMessage)
            => new()
            {
                Success = false,
                ErrorMessage = errorMessage
            };

        public static RefundOperationResult Succeeded(Guid? refundId, float refundAmount)
            => new()
            {
                Success = true,
                RefundId = refundId,
                RefundAmount = refundAmount
            };
    }
}
