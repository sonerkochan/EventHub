using EventHub.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.Areas.Moderator.Controllers
{
    public class ApplicationController : BaseController
    {
        private readonly IApplicationService applicationService;

        public ApplicationController(IApplicationService _applicationService)
        {
            applicationService = _applicationService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await applicationService.GetAllPendingAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await applicationService.ApproveAsync(id, adminId);
            TempData["Success"] = "Application approved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string comment)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await applicationService.RejectAsync(id, adminId, comment);
            TempData["Success"] = "Application rejected.";
            return RedirectToAction(nameof(Index));
        }
    }
}