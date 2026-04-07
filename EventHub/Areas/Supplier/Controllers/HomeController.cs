using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Supplier.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet]
        public IActionResult Index() => View();
    }
}
