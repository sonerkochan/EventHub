using EventHub.Core.Contracts;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Admin.Controllers
{
    public class ApplicationController : BaseController
    {
        private readonly IApplicationService applicationService;
        private readonly UserManager<User> userManager;

        public ApplicationController(IApplicationService _applicationService, UserManager<User> _userManager)
        {
            applicationService = _applicationService;
            userManager = _userManager;
        }

        // List all pending
        public async Task<IActionResult> Index()
        {
            var applications = await applicationService.GetAllPendingAsync();
            return View(applications);
        }

        // Approve
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var currentUser = await userManager.GetUserAsync(User);

            var result = await applicationService.ApproveAsync(id, currentUser.Id);

            TempData[result ? "Success" : "Error"] = result
                ? "Application approved!"
                : "Failed to approve application.";

            return RedirectToAction(nameof(Index));
        }

        // Reject
        [HttpPost]
        public async Task<IActionResult> Reject(int id, string comment)
        {
            var currentUser = await userManager.GetUserAsync(User);
            var result = await applicationService.RejectAsync(id, currentUser.Id, comment);

            TempData[result ? "Success" : "Error"] = result
                ? "Application rejected!"
                : "Failed to reject application.";

            return RedirectToAction(nameof(Index));
        }
    }
}