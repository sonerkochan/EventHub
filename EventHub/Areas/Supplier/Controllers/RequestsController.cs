using EventHub.Core.Contracts;
using EventHub.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace EventHub.Areas.Supplier.Controllers
{
    public class RequestsController : BaseController
    {
        private readonly ISupplierServiceCatalogService supplierServiceCatalogService;
        private readonly IStringLocalizer<SupplierResource> supplierLocalizer;

        public RequestsController(
            ISupplierServiceCatalogService _supplierServiceCatalogService,
            IStringLocalizer<SupplierResource> _supplierLocalizer)
        {
            supplierServiceCatalogService = _supplierServiceCatalogService;
            supplierLocalizer = _supplierLocalizer;
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
                ? supplierLocalizer["Supplier.Requests.Accepted"].Value
                : supplierLocalizer["Supplier.Requests.AcceptFailed"].Value;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(int id, string? responseComment)
        {
            var supplierId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var declined = await supplierServiceCatalogService.DeclineRequestAsync(id, supplierId, supplierId, responseComment);

            TempData[declined ? "Success" : "Error"] = declined
                ? supplierLocalizer["Supplier.Requests.Declined"].Value
                : supplierLocalizer["Supplier.Requests.DeclineFailed"].Value;

            return RedirectToAction(nameof(Index));
        }
    }
}
