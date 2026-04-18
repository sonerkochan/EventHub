using EventHub.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.Areas.Client.Controllers
{
    public class TicketsController : BaseController
    {
        private readonly ITicketService ticketService;

        public TicketsController(ITicketService _ticketService)
        {
            ticketService = _ticketService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var model = await ticketService.GetUserTicketsAsync(userId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var model = await ticketService.GetTicketByIdAsync(id, userId);
            if (model == null) return NotFound();
            return View(model);
        }
    }
}