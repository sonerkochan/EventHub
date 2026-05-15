using System.ComponentModel.DataAnnotations;

namespace EventHub.Core.Models.User
{
    public class EditUserViewModel
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }
    }
}
