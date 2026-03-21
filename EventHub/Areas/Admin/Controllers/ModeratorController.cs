using EventHub.Core.Contracts;
using EventHub.Core.Models.Moderator;
using EventHub.Core.Models.Venue;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using EventHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EventHub.Areas.Admin.Controllers
{
    public class ModeratorController : BaseController
    {
        private readonly IModeratorService moderatorService;

        public ModeratorController(IModeratorService _moderatorService)
        {
            moderatorService = _moderatorService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await moderatorService.GetAllModeratorsAsync();
            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new AddModeratorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddModeratorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await moderatorService.CreateModeratorAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Failed to create moderator. Username or email may already be taken.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var model = await moderatorService.GetModeratorByIdAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditModeratorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await moderatorService.EditModeratorAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Failed to update moderator.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable(string id)
        {
            await moderatorService.SetActiveStatusAsync(id, false);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enable(string id)
        {
            await moderatorService.SetActiveStatusAsync(id, true);
            return RedirectToAction(nameof(Index));
        }
    }
}
