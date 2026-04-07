using EventHub.Core.Contracts;
using EventHub.Core.Models.ApplicationForm;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.Areas.Client.Controllers
{
    public class ApplicationController : BaseController
    {
        private readonly IApplicationService applicationService;

        public ApplicationController(IApplicationService _applicationService)
        {
            applicationService = _applicationService;
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
                TempData["Error"] = "You already have a pending application.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Application submitted!";
            return RedirectToAction(nameof(Index));
        }
    }
}