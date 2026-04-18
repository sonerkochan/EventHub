using EventHub.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.Areas.Organizer.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IDashboardService dashboardService;

        public HomeController(IDashboardService _dashboardService)
        {
            dashboardService = _dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var model = await dashboardService.GetOrganizerDashboardAsync(userId);
            return View(model);
        }
    }
}
