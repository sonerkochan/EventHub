using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EzyShape.Core.Models.User
{
    /// <summary>
    /// View model to pass data while registering a new client.
    /// </summary>
    public class ClientRegisterViewModel
    {
        [Required(ErrorMessage = "The username field is required!")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "The email field is required!")]
        [EmailAddress]

        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "The password field is required!")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your password!")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
