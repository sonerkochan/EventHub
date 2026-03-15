using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Client.Controllers
{
    /// <summary>
    /// Base controller class that all other controllers in Client area inherit in order to lock authorization.
    /// </summary>
    [Area("Client")]
    [Route("/Client/[controller]/[Action]/{id?}")]
    [Authorize(Roles = "Client")]
    public class BaseController : Controller
    {

    }
}
