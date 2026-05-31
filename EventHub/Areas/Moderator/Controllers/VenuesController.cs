using EventHub.Core.Contracts;
using EventHub.Core.Models.Venue;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.Areas.Moderator.Controllers
{
    public class VenuesController : BaseController
    {
        private readonly IVenueService venueService;

        public VenuesController(IVenueService _venueService)
        {
            venueService = _venueService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await venueService.GetAllVenuesAsync();
            return View(model);
        }

        [HttpGet]
        public IActionResult CreatePartial()
        {
            return PartialView("_CreateModal", new AddVenueViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddVenueViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateModal", model);
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await venueService.AddVenueAsync(model, userId);

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> EditPartial(Guid id)
        {
            var model = await venueService.GetForEditAsync(id);
            if (model == null) return NotFound();

            return PartialView("_EditModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditVenueViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditModal", model);
            }

            var success = await venueService.UpdateAsync(model);
            if (!success) return NotFound();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            await venueService.DeactivateAsync(id);
            return Json(new { success = true });
        }
    }
}
