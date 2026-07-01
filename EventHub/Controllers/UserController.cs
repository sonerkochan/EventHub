using System.Security.Claims;
using EventHub.Core.Contracts;
using EventHub.Core.Models.User;
using EventHub.Infrastructure.Data.Models;
using EventHub.Localization;
using EzyShape.Core.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace EventHub.Controllers
{
    /// <summary>
    /// The controller is responsible for user management.
    /// </summary>
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IExternalAuthService externalAuthService;
        private readonly IStringLocalizer<MessagesResource> messagesLocalizer;

        /// <summary>
        /// Constructor for the user controller.
        /// </summary>
        public UserController(
            UserManager<User> _userManager,
            SignInManager<User> _signInManager,
            RoleManager<IdentityRole> _roleManager,
            IExternalAuthService _externalAuthService,
            IStringLocalizer<MessagesResource> _messagesLocalizer
        )
        {
            userManager = _userManager;
            signInManager = _signInManager;
            roleManager = _roleManager;
            externalAuthService = _externalAuthService;
            messagesLocalizer = _messagesLocalizer;
        }

        /// <summary>
        /// The register method for the controller.
        /// </summary>
        /// <returns>An empty 'RegisterViewModel'.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ClientRegisterViewModel();

            return View(model);
        }

        /// <summary>
        /// The register method for the controller.
        /// </summary>
        /// <param name="model">'ClientRegisterViewModel' filled with data in the registration form.</param>
        /// <returns>Registers the user in the system if everything is okay.</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(ClientRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User()
            {
                Email = model.Email,
                UserName = model.UserName,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleName = "Client";
                var roleExists = await roleManager.RoleExistsAsync(roleName);

                if (roleExists)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }

                return RedirectToAction("Login", "User");
            }

            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("", item.Description);
            }

            return View(model);
        }

        /// <summary>
        /// The login action for the controller.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel();

            return View(model);
        }

        /// <summary>
        /// The login action for the controller.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByNameAsync(model.Username);

            if (user != null)
            {
                if (!user.IsActive)
                {
                    ModelState.AddModelError("", messagesLocalizer["Messages.Auth.AccountDeactivated"]);
                    return View(model);
                }

                var result = await signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    false,
                    false
                );

                if (result.Succeeded)
                {
                    user.LastLoginAt = DateTime.UtcNow.AddHours(3);
                    user.LastLoginIP = GetClientIp();
                    user.LastLoginDevice = GetDevice();
                    user.LastOnline = user.LastLoginAt.Value;

                    await userManager.UpdateAsync(user);

                    return await RedirectAfterSignInAsync(user, null);
                }
            }

            ModelState.AddModelError("", messagesLocalizer["Messages.Auth.InvalidCredentials"]);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            if (!string.Equals(provider, GoogleDefaults.AuthenticationScheme, StringComparison.Ordinal))
            {
                return BadRequest();
            }

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "User", new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (!string.IsNullOrWhiteSpace(remoteError))
            {
                AddExternalLoginError(ExternalLoginProcessStatus.Failed, remoteError);
                return View(nameof(Login), new LoginViewModel());
            }

            var loginInfo = await signInManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                AddExternalLoginError(ExternalLoginProcessStatus.Failed);
                return View(nameof(Login), new LoginViewModel());
            }

            var result = await externalAuthService.HandleExternalLoginCallbackAsync(
                loginInfo,
                GetClientIp(),
                GetDevice());

            if (result.Succeeded && result.User != null)
            {
                await signInManager.SignInAsync(
                    result.User,
                    isPersistent: false,
                    loginInfo.LoginProvider);

                return await RedirectAfterSignInAsync(result.User, returnUrl);
            }

            if (result.Status == ExternalLoginProcessStatus.RequiresConfirmation)
            {
                return RedirectToAction(nameof(ExternalLoginConfirmation), new { returnUrl });
            }

            AddExternalLoginError(result.Status, result.Error);
            return View(nameof(Login), new LoginViewModel());
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginConfirmation(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home");
            }

            var loginInfo = await signInManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                AddExternalLoginError(ExternalLoginProcessStatus.Failed);
                return View(nameof(Login), new LoginViewModel());
            }

            var email = GetExternalEmail(loginInfo);
            if (string.IsNullOrWhiteSpace(email))
            {
                AddExternalLoginError(ExternalLoginProcessStatus.EmailUnavailable);
                return View(nameof(Login), new LoginViewModel());
            }

            return View(new ExternalLoginConfirmationViewModel
            {
                Email = email,
                Provider = loginInfo.ProviderDisplayName ?? loginInfo.LoginProvider
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(
            ExternalLoginConfirmationViewModel model,
            string? returnUrl = null)
        {
            var loginInfo = await signInManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                AddExternalLoginError(ExternalLoginProcessStatus.Failed);
                return View(nameof(Login), new LoginViewModel());
            }

            model.Email = GetExternalEmail(loginInfo) ?? model.Email;
            model.Provider = loginInfo.ProviderDisplayName ?? loginInfo.LoginProvider;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await externalAuthService.ConfirmExternalLoginAsync(
                loginInfo,
                model,
                GetClientIp(),
                GetDevice());

            if (result.Succeeded && result.User != null)
            {
                await signInManager.SignInAsync(
                    result.User,
                    isPersistent: false,
                    loginInfo.LoginProvider);

                return await RedirectAfterSignInAsync(result.User, returnUrl);
            }

            AddExternalLoginError(result.Status, result.Error);
            return View(model);
        }

        /// <summary>
        /// The log out method of the controller.
        /// </summary>
        /// <returns>Returns the user to the index page.</returns>
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await userManager.UpdateAsync(user);
                }
            }

            await signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        private async Task<IActionResult> RedirectAfterSignInAsync(User user, string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            var roles = await userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            return !string.IsNullOrEmpty(role)
                ? RedirectToAction("Index", "Home", new { area = role })
                : RedirectToAction("Index", "Home");
        }

        private void AddExternalLoginError(ExternalLoginProcessStatus status, string? error = null)
        {
            var messageKey = status switch
            {
                ExternalLoginProcessStatus.AccountInactive => "Messages.Auth.AccountDeactivated",
                ExternalLoginProcessStatus.EmailUnavailable => "Messages.Auth.ExternalEmailUnavailable",
                ExternalLoginProcessStatus.EmailNotVerified => "Messages.Auth.ExternalEmailNotVerified",
                ExternalLoginProcessStatus.DuplicateUserName => "Messages.Auth.DuplicateUserName",
                _ => "Messages.Auth.ExternalLoginFailed"
            };

            ModelState.AddModelError("", error ?? messagesLocalizer[messageKey]);
        }

        private static string? GetExternalEmail(ExternalLoginInfo loginInfo)
            => loginInfo.Principal.FindFirstValue(ClaimTypes.Email)
               ?? loginInfo.Principal.FindFirstValue("email");

        private string? GetClientIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private string GetDevice()
        {
            return Request.Headers["User-Agent"].ToString();
        }
    }
}
