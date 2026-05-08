using Microsoft.AspNetCore.Mvc;
using EventHub.Areas.Supplier.Models;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Areas.Supplier.Controllers
{
    public class ServicesController : BaseController
    {
        private readonly ApplicationDbContext _db;

        public ServicesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var services = await _db.SupplierServices
                .Where(s => s.SupplierId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

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

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await _db.SupplierServices.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (entity == null) return NotFound();
            if (entity.SupplierId != userId && !User.IsInRole("Admin")) return Forbid();

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

            return RedirectToAction("Index");
        }
    }
}
