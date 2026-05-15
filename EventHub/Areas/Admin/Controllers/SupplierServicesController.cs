using EventHub.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Admin.Controllers
{
    public class SupplierServicesController : BaseController
    {
        private readonly IAdminSupplierServiceService supplierServices;

        public SupplierServicesController(IAdminSupplierServiceService _supplierServices)
        {
            supplierServices = _supplierServices;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? status = null, string? q = null)
        {
            var rows = await supplierServices.GetAllAsync(status, q);
            ViewBag.StatusFilter = status;
            ViewBag.SearchTerm = q;
            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id)
        {
            var ok = await supplierServices.HideAsync(id);
            return Json(new { success = ok });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unhide(int id)
        {
            var ok = await supplierServices.UnhideAsync(id);
            return Json(new { success = ok });
        }
    }
}
