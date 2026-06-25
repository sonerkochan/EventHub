using EventHub.Core.Contracts;
using EventHub.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace EventHub.Areas.Client.Controllers
{
    public class TicketsController : BaseController
    {
        private readonly ITicketService ticketService;
        private readonly IRefundService refundService;
        private readonly IStringLocalizer<MessagesResource> messagesLocalizer;

        public TicketsController(
            ITicketService ticketService,
            IRefundService refundService,
            IStringLocalizer<MessagesResource>? messagesLocalizer = null)
        {
            this.ticketService = ticketService;
            this.refundService = refundService;
            this.messagesLocalizer = messagesLocalizer ?? new FallbackStringLocalizer<MessagesResource>();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRefund(Guid ticketId, string? reason)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdValue))
            {
                return Unauthorized();
            }

            var result = await refundService.RequestTicketRefundAsync(ticketId, Guid.Parse(userIdValue), reason);

            if (result.Success)
            {
                TempData["Success"] = messagesLocalizer["Messages.Refund.Requested"].Value;
            }
            else
            {
                var key = result.ErrorMessage ?? "Messages.Refund.RequestFailed";
                TempData["Error"] = messagesLocalizer[key].Value;
            }

            return RedirectToAction(nameof(Details), new { id = ticketId });
        }
    }
}
