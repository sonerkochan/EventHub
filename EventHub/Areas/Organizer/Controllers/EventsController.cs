using EventHub.Core.Contracts;
using EventHub.Core.Models.Event;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Models;
using EventHub.Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace EventHub.Areas.Organizer.Controllers
{
    public class EventsController : BaseController
    {
        private readonly UserManager<User> userManager;
        private readonly IEventService eventService;
        private readonly IRoomService roomService;
        private readonly IPhotoService photoService;
        private readonly IStringLocalizer<MessagesResource> messagesLocalizer;

        public EventsController(
            UserManager<User> _userManager,
            IEventService _eventService,
            IRoomService _roomService,
            IPhotoService _photoService,
            IStringLocalizer<MessagesResource>? messagesLocalizer = null)
        {
            userManager = _userManager;
            eventService = _eventService;
            roomService = _roomService;
            photoService = _photoService;
            this.messagesLocalizer = messagesLocalizer ?? new FallbackStringLocalizer<MessagesResource>();
        }
        public async Task<IActionResult> Index()
        {
            var userIdValue = userManager.GetUserId(User);
            if (userIdValue == null)
            {
                return Unauthorized();
            }

            Guid userId = Guid.Parse(userIdValue);
            var model = await eventService.GetOrganizersEventsAsync(userId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateEventViewModel
            {
                AvailableRooms = await BuildRoomSelectList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            ValidateCoverImageUrl(model.CoverPhotoUpload, model.CoverImageUrl);
            if (!ModelState.IsValid)
            {
                model.AvailableRooms = await BuildRoomSelectList();
                return View(model);
            }

            await ApplyCoverImageAsync(model);
            if (!ModelState.IsValid)
            {
                model.AvailableRooms = await BuildRoomSelectList();
                return View(model);
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await eventService.CreateAsync(model, userId);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var model = await eventService.GetEventForEditAsync(id);
            if (model == null) return NotFound();

            model.AvailableRooms = await BuildRoomSelectList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditEventViewModel model)
        {
            ValidateCoverImageUrl(model.CoverPhotoUpload, model.CoverImageUrl);
            if (!ModelState.IsValid)
            {
                model.AvailableRooms = await BuildRoomSelectList();
                return View(model);
            }

            await ApplyCoverImageAsync(model);
            if (!ModelState.IsValid)
            {
                model.AvailableRooms = await BuildRoomSelectList();
                return View(model);
            }

            var success = await eventService.UpdateAsync(model);
            if (!success) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var model = await eventService.GetEventByIdAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            await eventService.DeactivateAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(Guid id)
        {
            await eventService.PublishAsync(id);
            TempData["Success"] = messagesLocalizer["Messages.Event.Published"].Value;
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> BuildRoomSelectList()
        {
            var rooms = await roomService.GetAllRoomsAsync();
            return rooms.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = $"{r.Name} (cap. {r.Capacity})"
            });
        }

        private async Task ApplyCoverImageAsync(CreateEventViewModel model)
        {
            if (model.CoverPhotoUpload?.Length > 0)
            {
                await UploadCoverPhotoAsync(model.CoverPhotoUpload, id => model.CoverPhotoId = id);
                model.CoverImageUrl = null;
                return;
            }

            model.CoverImageUrl = string.IsNullOrWhiteSpace(model.CoverImageUrl)
                ? null
                : model.CoverImageUrl.Trim();
        }

        private async Task ApplyCoverImageAsync(EditEventViewModel model)
        {
            if (model.CoverPhotoUpload?.Length > 0)
            {
                await UploadCoverPhotoAsync(model.CoverPhotoUpload, id => model.CoverPhotoId = id);
                model.CoverImageUrl = null;
                return;
            }

            model.CoverImageUrl = string.IsNullOrWhiteSpace(model.CoverImageUrl)
                ? null
                : model.CoverImageUrl.Trim();
        }

        private async Task UploadCoverPhotoAsync(IFormFile file, Action<Guid?> setPhotoId)
        {
            try
            {
                setPhotoId(await photoService.UploadPhotoAsync(file));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("CoverPhotoUpload", ex.Message);
            }
        }

        private void ValidateCoverImageUrl(IFormFile? upload, string? value)
        {
            if (upload?.Length > 0 || EventCoverImageResolver.IsValidExternalUrl(value))
            {
                return;
            }

            ModelState.AddModelError("CoverImageUrl", messagesLocalizer["Messages.Event.CoverUrlInvalid"]);
        }
    }
}
