using EventHub.Core.Contracts;
using EventHub.Core.Models.ApplicationForm;
using EventHub.Localization;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace EventHub.Areas.Client.Controllers
{
    public class ApplicationController : BaseController
    {
        private readonly IApplicationService applicationService;
        private readonly IStringLocalizer<MessagesResource> messagesLocalizer;

        public ApplicationController(
            IApplicationService _applicationService,
            IStringLocalizer<MessagesResource> _messagesLocalizer)
        {
            applicationService = _applicationService;
            messagesLocalizer = _messagesLocalizer;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Apply(ApplicationType type)
        {
            var model = new ApplicationFormViewModel
            {
                Type = type
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Apply(ApplicationFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await applicationService.ApplyAsync(userId!, model);

            if (!result)
            {
                TempData["Error"] = messagesLocalizer["Messages.Application.AlreadyPending"].Value;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = messagesLocalizer["Messages.Application.Submitted"].Value;
            return RedirectToAction(nameof(Index));
        }
    }
}
