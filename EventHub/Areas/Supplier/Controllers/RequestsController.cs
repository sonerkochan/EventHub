using EventHub.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.Areas.Supplier.Controllers
{
    public class RequestsController : BaseController
    {
        private readonly ISupplierServiceCatalogService supplierServiceCatalogService;

        public RequestsController(ISupplierServiceCatalogService _supplierServiceCatalogService)
        {
            supplierServiceCatalogService = _supplierServiceCatalogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var supplierId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var model = await supplierServiceCatalogService.GetRequestsForSupplierAsync(supplierId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id, string? responseComment)
        {
            var supplierId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var accepted = await supplierServiceCatalogService.AcceptRequestAsync(id, supplierId, supplierId, responseComment);

            TempData[accepted ? "Success" : "Error"] = accepted
                ? "Service request accepted."
                : "Unable to accept this request.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(int id, string? responseComment)
        {
            var supplierId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var declined = await supplierServiceCatalogService.DeclineRequestAsync(id, supplierId, supplierId, responseComment);

            TempData[declined ? "Success" : "Error"] = declined
                ? "Service request declined."
                : "Unable to decline this request.";

            return RedirectToAction(nameof(Index));
        }
    }
}
