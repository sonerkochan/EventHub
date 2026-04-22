using System.Security.Claims;
using EventHub.Core.Models.User;
using EventHub.Infrastructure.Data.Models;
using EzyShape.Core.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        /// <summary>
        /// Constructor for the user controller.
        /// </summary>
        public UserController(
            UserManager<User> _userManager,
            SignInManager<User> _signInManager,
            RoleManager<IdentityRole> _roleManager
        )
        {
            userManager = _userManager;
            signInManager = _signInManager;
            roleManager = _roleManager;
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

            var user = new User() { Email = model.Email, UserName = model.UserName };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleName = "Client";
                var roleExists = await roleManager.RoleExistsAsync(roleName);

                if (roleExists)
                {
                    var roleResult = await userManager.AddToRoleAsync(user, roleName);
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
                var result = await signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    false,
                    false
                );

                if (result.Succeeded)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    var role = roles.FirstOrDefault();

                    return !string.IsNullOrEmpty(role)
                        ? RedirectToAction("Index", "Home", new { area = role })
                        : RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Invalid login");

            return View(model);
        }

        /// <summary>
        /// The log out method of the controller.
        /// </summary>
        /// <returns>Returns the user to the index page.</returns>
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId);
            await userManager.UpdateAsync(user);

            await signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
