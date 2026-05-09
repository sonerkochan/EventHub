using EventHub.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.Areas.Organizer.Controllers
{
    public class ServicesController : BaseController
    {
        private readonly ISupplierServiceCatalogService supplierServiceCatalogService;

        public ServicesController(ISupplierServiceCatalogService _supplierServiceCatalogService)
        {
            supplierServiceCatalogService = _supplierServiceCatalogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var model = await supplierServiceCatalogService.SearchServicesAsync(searchTerm, userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rent(int serviceId, string? message, string? searchTerm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var created = await supplierServiceCatalogService.RequestServiceAsync(serviceId, userId, message);

            TempData[created ? "Success" : "Error"] = created
                ? "Service request sent to the supplier."
                : "Unable to request this service. You may already have a pending request.";

            return RedirectToAction(nameof(Index), new { searchTerm });
        }
    }
}
