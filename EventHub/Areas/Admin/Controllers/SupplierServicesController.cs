using EventHub.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Admin.Controllers
{
    public class SupplierServicesController : BaseController
    {
        private const int DefaultPageSize = 10;
        private static readonly int[] PageSizeOptions = { 10, 25, 50, 100, 200 };

        private readonly IAdminSupplierServiceService supplierServices;

        public SupplierServicesController(IAdminSupplierServiceService _supplierServices)
        {
            supplierServices = _supplierServices;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? status = null, string? q = null, int page = 1, int size = DefaultPageSize)
        {
            var all = (await supplierServices.GetAllAsync(status, q)).ToList();

            var totalCount = all.Count;
            size = PageSizeOptions.Contains(size) ? size : DefaultPageSize;
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)size));
            page = Math.Clamp(page, 1, totalPages);
            var pageItems = all.Skip((page - 1) * size).Take(size).ToList();

            ViewBag.StatusFilter = status;
            ViewBag.SearchTerm = q;
            ViewBag.Page = page;
            ViewBag.PageSize = size;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSizeOptions = PageSizeOptions;
            ViewBag.AllRows = all; // for summary cards (full filtered set, not just the page)

            return View(pageItems);
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
