using EventHub.Core.Contracts;
using EventHub.Core.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseController
    {
        private const int DefaultPageSize = 10;
        private static readonly int[] PageSizeOptions = { 10, 25, 50, 100, 200 };

        private readonly IUserService userService;

        public UsersController(IUserService _userService)
        {
            userService = _userService;
        }

        public async Task<IActionResult> Index(string? role = null, int page = 1, int size = DefaultPageSize)
        {
            var all = (await userService.GetAllUsersAsync(role)).ToList();

            var totalCount = all.Count;
            size = PageSizeOptions.Contains(size) ? size : DefaultPageSize;
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)size));
            page = Math.Clamp(page, 1, totalPages);
            var pageItems = all.Skip((page - 1) * size).Take(size).ToList();

            ViewBag.RoleFilter = role;
            ViewBag.Page = page;
            ViewBag.PageSize = size;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSizeOptions = PageSizeOptions;
            ViewBag.AllUsers = all; // for summary cards (full filtered set, not just the page)

            return View(pageItems);
        }

        [HttpGet]
        public IActionResult CreatePartial()
        {
            return PartialView("_CreateModal", new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateModal", model);
            }

            var (success, error) = await userService.CreateUserAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Failed to create user.");
                return PartialView("_CreateModal", model);
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> EditPartial(string id)
        {
            var model = await userService.GetForEditAsync(id);
            if (model == null) return NotFound();

            return PartialView("_EditModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditModal", model);
            }

            var (success, error) = await userService.UpdateUserAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Failed to update user.");
                return PartialView("_EditModal", model);
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> DetailsPartial(string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return PartialView("_DetailsModal", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            var ok = await userService.DeactivateUserAsync(id);
            return Json(new { success = ok });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(string id)
        {
            var ok = await userService.ReactivateUserAsync(id);
            return Json(new { success = ok });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var ok = await userService.DeleteUserAsync(id);
            return Json(new { success = ok });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(string userId, string role)
        {
            var ok = await userService.AddRoleToUserAsync(userId, role);
            return Json(new { success = ok });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            var ok = await userService.RemoveRoleFromUserAsync(userId, role);
            return Json(new { success = ok });
        }
    }
}
