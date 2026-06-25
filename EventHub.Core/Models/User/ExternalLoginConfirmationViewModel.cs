using System.ComponentModel.DataAnnotations;

namespace EventHub.Core.Models.User
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Validation.User.Username.Required")]
        public string UserName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        public string Provider { get; set; } = null!;
    }
}
