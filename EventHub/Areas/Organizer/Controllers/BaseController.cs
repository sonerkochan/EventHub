using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Organizer.Controllers
{
    [Area("Organizer")]
    [Route("/Organizer/[controller]/[Action]/{id?}")]
    [Authorize(Roles = "Organizer,Admin")]
    public class BaseController : Controller { }
}
