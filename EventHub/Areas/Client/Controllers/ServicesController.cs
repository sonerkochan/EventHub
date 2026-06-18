using EventHub.Core.Contracts;
using EventHub.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace EventHub.Areas.Client.Controllers
{
    public class ServicesController : BaseController
    {
        private readonly ISupplierServiceCatalogService supplierServiceCatalogService;
        private readonly IStringLocalizer<MessagesResource> messagesLocalizer;

        public ServicesController(
            ISupplierServiceCatalogService _supplierServiceCatalogService,
            IStringLocalizer<MessagesResource> messagesLocalizer)
        {
            supplierServiceCatalogService = _supplierServiceCatalogService;
            this.messagesLocalizer = messagesLocalizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var model = await supplierServiceCatalogService.SearchServicesAsync(searchTerm, userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rent(int serviceId, string? message, string? searchTerm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var created = await supplierServiceCatalogService.RequestServiceAsync(serviceId, userId, message);

            TempData[created ? "Success" : "Error"] = created
                ? messagesLocalizer["Messages.Service.RequestCreated"].Value
                : messagesLocalizer["Messages.Service.RequestCreateFailed"].Value;

            return RedirectToAction(nameof(Index), new { searchTerm });
        }
    }
}
