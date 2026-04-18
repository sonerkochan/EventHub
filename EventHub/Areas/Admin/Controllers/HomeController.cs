using EventHub.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Admin.Controllers
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
            var model = await dashboardService.GetAdminDashboardAsync();
            return View(model);
        }
    }
}
