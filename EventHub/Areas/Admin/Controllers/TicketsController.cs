using EventHub.Core.Contracts;
using EventHub.Core.Models.Admin;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.Areas.Admin.Controllers
{
    public class TicketsController : BaseController
    {
        private const int DefaultPageSize = 10;
        private static readonly int[] PageSizeOptions = { 10, 25, 50, 100, 200 };

        private readonly ITicketService ticketService;

        public TicketsController(ITicketService _ticketService)
        {
            ticketService = _ticketService;
        }

        public async Task<IActionResult> Index(
            string? status = null,
            string? sort = null,
            string? dir = null,
            int page = 1,
            int size = DefaultPageSize)
        {
            TicketStatus? statusFilter = null;
            if (!string.IsNullOrWhiteSpace(status)
                && Enum.TryParse<TicketStatus>(status, ignoreCase: true, out var parsed))
            {
                statusFilter = parsed;
            }

            var all = (await ticketService.GetAllForAdminAsync(statusFilter)).ToList();

            sort = string.IsNullOrWhiteSpace(sort) ? "event" : sort.ToLowerInvariant();
            var descending = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase)
                             || string.IsNullOrWhiteSpace(dir) && (sort == "event" || sort == "created" || sort == "number");

            IEnumerable<AdminTicketRow> sorted = sort switch
            {
                "number" => descending
                    ? all.OrderByDescending(t => t.TicketNumber)
                    : all.OrderBy(t => t.TicketNumber),
                "seat" => descending
                    ? all.OrderByDescending(t => t.SeatNumber)
                    : all.OrderBy(t => t.SeatNumber),
                "buyer" => descending
                    ? all.OrderByDescending(t => t.BuyerDisplay)
                    : all.OrderBy(t => t.BuyerDisplay),
                "status" => descending
                    ? all.OrderByDescending(t => t.Status)
                    : all.OrderBy(t => t.Status),
                "price" => descending
                    ? all.OrderByDescending(t => t.Price)
                    : all.OrderBy(t => t.Price),
                "created" => descending
                    ? all.OrderByDescending(t => t.PurchasedAt > DateTime.MinValue ? t.PurchasedAt : t.ReservedAt)
                    : all.OrderBy(t => t.PurchasedAt > DateTime.MinValue ? t.PurchasedAt : t.ReservedAt),
                _ => descending
                    ? all.OrderByDescending(t => t.EventStart).ThenBy(t => t.EventName).ThenByDescending(t => t.TicketNumber)
                    : all.OrderBy(t => t.EventStart).ThenBy(t => t.EventName).ThenByDescending(t => t.TicketNumber)
            };

            var totalCount = all.Count;
            size = PageSizeOptions.Contains(size) ? size : DefaultPageSize;
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)size));
            page = Math.Clamp(page, 1, totalPages);
            var pageItems = sorted.Skip((page - 1) * size).Take(size).ToList();

            ViewBag.StatusFilter = statusFilter?.ToString();
            ViewBag.Sort = sort;
            ViewBag.Dir = descending ? "desc" : "asc";
            ViewBag.Page = page;
            ViewBag.PageSize = size;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSizeOptions = PageSizeOptions;
            ViewBag.AllRows = all; // for summary cards (full filtered set, not just the page)

            return View(pageItems);
        }

        [HttpGet]
        public async Task<IActionResult> LookupPartial(string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return PartialView("_LookupModal", (AdminTicketLookupDto?)null);
            }

            var dto = await ticketService.LookupAsync(q);
            ViewBag.SearchedQuery = q.Trim();
            return PartialView("_LookupModal", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(Guid ticketId)
        {
            var processedBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ok = await ticketService.AdminRefundTicketAsync(ticketId, processedBy);
            return Json(new { success = ok });
        }

        [HttpGet]
        public async Task<IActionResult> EditPartial(Guid id)
        {
            var model = await ticketService.GetForAdminEditAsync(id);
            if (model == null) return NotFound();
            return PartialView("_EditModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] AdminTicketEditRequest request)
        {
            var (success, error) = await ticketService.AdminUpdateTicketAsync(request);
            return Json(new { success, message = error });
        }
    }
}
