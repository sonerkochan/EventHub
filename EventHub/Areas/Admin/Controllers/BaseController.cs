using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Admin.Controllers
{
    /// <summary>
    /// Base controller class that all other controllers in Admin area inherit in order to lock authorization.
    /// </summary>
    [Area("Admin")]
    [Route("/Admin/[controller]/[Action]/{id?}")]
    [Authorize(Roles = "Admin")]
    public class BaseController : Controller
    {

    }
}
