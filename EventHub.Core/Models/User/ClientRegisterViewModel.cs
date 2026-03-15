using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EzyShape.Core.Models.User
{
    /// <summary>
    /// View model to pass data while registering a new client.
    /// </summary>
    public class ClientRegisterViewModel
    {
        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
