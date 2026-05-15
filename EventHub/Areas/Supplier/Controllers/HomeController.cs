using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EventHub.Core.Contracts;
using EventHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using EventHub.Infrastructure.Data.Models;
using System.Linq;

namespace EventHub.Areas.Supplier.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly ISupplierServiceCatalogService _supplierService;

        public HomeController(ApplicationDbContext db, ISupplierServiceCatalogService supplierService)
        {
            _db = db;
            _supplierService = supplierService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            var servicesCount = await _db.SupplierServices.CountAsync(s => s.SupplierId == userId);
            var requests = await _supplierService.GetRequestsForSupplierAsync(userId);
            
            ViewBag.ServicesCount = servicesCount;
            ViewBag.PendingRequests = requests.Count(r => r.Status == ServiceRentalRequestStatus.Pending);
            ViewBag.CompletedRequests = requests.Count(r => r.Status == ServiceRentalRequestStatus.Accepted || r.Status == ServiceRentalRequestStatus.Declined);

            return View();
        }
    }
}
