using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Admin.Controllers
{
    public class HomeController : BaseController
    {

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
