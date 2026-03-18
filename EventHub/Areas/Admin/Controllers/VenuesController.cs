using EventHub.Core.Contracts;
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
        public IActionResult Create()
        {
            return View(new AddVenueViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddVenueViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            await venueService.AddVenueAsync(model, userId);

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
