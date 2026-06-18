using EventHub.Core.Contracts;
using EventHub.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace EventHub.Areas.Admin.Controllers
{
    public class ApplicationController : BaseController
    {
        private readonly IApplicationService applicationService;
        private readonly IStringLocalizer<MessagesResource> messagesLocalizer;

        public ApplicationController(
            IApplicationService _applicationService,
            IStringLocalizer<MessagesResource>? messagesLocalizer = null)
        {
            applicationService = _applicationService;
            this.messagesLocalizer = messagesLocalizer ?? new FallbackStringLocalizer<MessagesResource>();
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
            TempData["Success"] = messagesLocalizer["Messages.Application.Approved"].Value;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string comment)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await applicationService.RejectAsync(id, adminId, comment);
            TempData["Success"] = messagesLocalizer["Messages.Application.Rejected"].Value;
            return RedirectToAction(nameof(Index));
        }
    }
}
