using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Moderator.Controllers
{
    /// <summary>
    /// Base controller class that all other controllers in Moderator area inherit in order to lock authorization.
    /// </summary>
    [Area("Moderator")]
    [Route("/Moderator/[controller]/[Action]/{id?}")]
    [Authorize(Roles = "Moderator")]
    public class BaseController : Controller
    {
        [HttpGet]
        public IActionResult Index() => View();
    }
}
