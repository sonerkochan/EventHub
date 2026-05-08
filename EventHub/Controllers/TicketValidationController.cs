using EventHub.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Controllers
{
    [AllowAnonymous]
    [Route("validate")]
    public class TicketValidationController : Controller
    {
        private readonly ITicketService ticketService;

        public TicketValidationController(ITicketService _ticketService)
        {
            ticketService = _ticketService;
        }

        [HttpGet("{hashedCode}")]
        public async Task<IActionResult> Index(string hashedCode)
        {
            var result = await ticketService.ValidateTicketAsync(hashedCode);
            if (result == null) return View("Invalid");
            return View(result);
        }
    }
}
