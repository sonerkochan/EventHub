using System;
using System.ComponentModel.DataAnnotations;

namespace EventHub.Core.Models.Refund
{
    public class CreateRefundViewModel
    {
        [Required]
        public Guid PaymentId { get; set; }

        [Required]
        [Range(0.01f, float.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public float Amount { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}
