using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EzyShape.Core.Models.User
{
    /// <summary>
    /// View model to pass data while registering a new client.
    /// </summary>
    public class ClientRegisterViewModel
    {
        [Required(ErrorMessage = "Validation.User.Username.Required")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Validation.User.Email.Required")]
        [EmailAddress]

        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Validation.User.Password.Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Validation.User.ConfirmPassword.Required")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Validation.Passwords.DoNotMatch")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
