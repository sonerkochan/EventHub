using EventHub.Core.Contracts;
using EventHub.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using RefundStatus = EventHub.Infrastructure.Data.Models.Refund.RefundStatus;

namespace EventHub.Areas.Organizer.Controllers
{
    public class RefundsController : BaseController
    {
        private readonly IRefundService refundService;
        private readonly IStringLocalizer<MessagesResource> messagesLocalizer;

        public RefundsController(
            IRefundService refundService,
            IStringLocalizer<MessagesResource>? messagesLocalizer = null)
        {
            this.refundService = refundService;
            this.messagesLocalizer = messagesLocalizer ?? new FallbackStringLocalizer<MessagesResource>();
        }

        [HttpGet]
        public async Task<IActionResult> Index(RefundStatus? statusFilter)
        {
            var organizerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var model = await refundService.GetRefundsForOrganizerAsync(organizerId, statusFilter);
            ViewBag.StatusFilter = statusFilter?.ToString();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid refundId)
        {
            var organizerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await refundService.ApproveTicketRefundAsync(refundId, organizerId);

            if (result.Success)
            {
                TempData["Success"] = messagesLocalizer["Messages.Refund.Approved"].Value;
            }
            else
            {
                var key = result.ErrorMessage ?? "Messages.Refund.ApproveFailed";
                TempData["Error"] = messagesLocalizer[key].Value;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid refundId, string? comment)
        {
            var organizerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await refundService.RejectTicketRefundAsync(refundId, organizerId, comment);

            if (result.Success)
            {
                TempData["Success"] = messagesLocalizer["Messages.Refund.Rejected"].Value;
            }
            else
            {
                var key = result.ErrorMessage ?? "Messages.Refund.RejectFailed";
                TempData["Error"] = messagesLocalizer[key].Value;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
