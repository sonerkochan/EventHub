using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class EmailVerificationToken
    {
        //[Key]
        public Guid TokenId { get; set; }
        public Guid UserId { get; set; }
        public string? Token { get; set; }
        public int ExpiresAt { get; set; }
        public DateTime? VerifiedAt { get; set; } = null;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
