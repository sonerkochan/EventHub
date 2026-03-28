using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Organizer.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet]
        public IActionResult Index() => View();
    }
}
