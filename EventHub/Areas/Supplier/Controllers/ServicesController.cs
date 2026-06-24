using Microsoft.AspNetCore.Mvc;
using EventHub.Areas.Supplier.Models;
using EventHub.Core.Contracts;
using EventHub.Localization;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace EventHub.Areas.Supplier.Controllers
{
    public class ServicesController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrencyDisplayService currencyDisplayService;
        private readonly IStringLocalizer<SupplierResource> supplierLocalizer;

        public ServicesController(
            ApplicationDbContext db,
            ICurrencyDisplayService _currencyDisplayService,
            IStringLocalizer<SupplierResource> _supplierLocalizer)
        {
            _db = db;
            currencyDisplayService = _currencyDisplayService;
            supplierLocalizer = _supplierLocalizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var services = await _db.SupplierServices
                .Where(s => s.SupplierId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            ViewBag.PriceTexts = await BuildPriceTextsAsync(services);
            return View(services);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var entity = new SupplierService
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                SupplierId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _db.SupplierServices.Add(entity);
            await _db.SaveChangesAsync();

            TempData["Success"] = supplierLocalizer["Supplier.Services.Created"].Value;

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await _db.SupplierServices.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (entity == null) return NotFound();
            if (entity.SupplierId != userId && !User.IsInRole("Admin")) return Forbid();

            var model = new ServiceCreateViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await _db.SupplierServices.FirstOrDefaultAsync(s => s.Id == model.Id && !s.IsDeleted);
            if (entity == null) return NotFound();
            if (entity.SupplierId != userId && !User.IsInRole("Admin")) return Forbid();

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.Price = model.Price;
            entity.UpdatedAt = DateTime.UtcNow;

            _db.SupplierServices.Update(entity);
            await _db.SaveChangesAsync();

            TempData["Success"] = supplierLocalizer["Supplier.Services.Updated"].Value;

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await _db.SupplierServices.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (entity == null) return NotFound();
            if (entity.SupplierId != userId && !User.IsInRole("Admin")) return Forbid();

            ViewBag.PriceText = entity.Price.HasValue
                ? (await currencyDisplayService.FormatAsync(entity.Price.Value)).Text
                : null;

            return View(entity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await _db.SupplierServices.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (entity == null) return NotFound();
            if (entity.SupplierId != userId && !User.IsInRole("Admin")) return Forbid();

            // Soft delete
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            _db.SupplierServices.Update(entity);
            await _db.SaveChangesAsync();

            TempData["Success"] = supplierLocalizer["Supplier.Services.Deleted"].Value;

            return RedirectToAction("Index");
        }

        private async Task<Dictionary<int, string>> BuildPriceTextsAsync(IEnumerable<SupplierService> services)
        {
            var priceTexts = new Dictionary<int, string>();

            foreach (var service in services.Where(s => s.Price.HasValue))
            {
                priceTexts[service.Id] = (await currencyDisplayService.FormatAsync(service.Price!.Value)).Text;
            }

            return priceTexts;
        }
    }
}
