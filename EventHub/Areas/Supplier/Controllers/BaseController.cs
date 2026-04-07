using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Supplier.Controllers
{
    [Area("Supplier")]
    [Route("/Supplier/[controller]/[Action]/{id?}")]
    [Authorize(Roles = "Organizer,Admin")]
    public class BaseController : Controller { }
}
