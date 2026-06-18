using EventHub.Core.Contracts;
using EventHub.Core.Models.Event;
using EventHub.Core.Services;
using EventHub.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace EventHub.Areas.Admin.Controllers
{
    public class EventsController : BaseController
    {
        private readonly IEventService eventService;
        private readonly IRoomService roomService;
        private readonly IPhotoService photoService;
        private readonly IStringLocalizer<MessagesResource> messagesLocalizer;

        public EventsController(
            IEventService _eventService,
            IRoomService _roomService,
            IPhotoService _photoService,
            IStringLocalizer<MessagesResource>? messagesLocalizer = null)
        {
            eventService = _eventService;
            roomService = _roomService;
            photoService = _photoService;
            this.messagesLocalizer = messagesLocalizer ?? new FallbackStringLocalizer<MessagesResource>();
        }

        public async Task<IActionResult> Index()
        {
            var model = await eventService.GetAllEventsAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreatePartial()
        {
            var model = new CreateEventViewModel
            {
                AvailableRooms = await BuildRoomSelectList()
            };
            return PartialView("_CreateModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            ValidateCoverImageUrl(model.CoverPhotoUpload, model.CoverImageUrl);
            if (!ModelState.IsValid)
            {
                model.AvailableRooms = await BuildRoomSelectList();
                return PartialView("_CreateModal", model);
            }

            await ApplyCoverImageAsync(model);
            if (!ModelState.IsValid)
            {
                model.AvailableRooms = await BuildRoomSelectList();
                return PartialView("_CreateModal", model);
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await eventService.CreateAsync(model, userId);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> EditPartial(Guid id)
        {
            var model = await eventService.GetEventForEditAsync(id);
            if (model == null) return NotFound();

            model.AvailableRooms = await BuildRoomSelectList();
            return PartialView("_EditModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditEventViewModel model)
        {
            ValidateCoverImageUrl(model.CoverPhotoUpload, model.CoverImageUrl);
            if (!ModelState.IsValid)
            {
                model.AvailableRooms = await BuildRoomSelectList();
                return PartialView("_EditModal", model);
            }

            await ApplyCoverImageAsync(model);
            if (!ModelState.IsValid)
            {
                model.AvailableRooms = await BuildRoomSelectList();
                return PartialView("_EditModal", model);
            }

            var success = await eventService.UpdateAsync(model);
            if (!success) return NotFound();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> DetailsPartial(Guid id)
        {
            var model = await eventService.GetEventByIdAsync(id);
            if (model == null) return NotFound();
            return PartialView("_DetailsModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            await eventService.DeactivateAsync(id);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(Guid id)
        {
            await eventService.PublishAsync(id);
            return Json(new { success = true });
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

            NormalizeCoverImageUrl(model.CoverImageUrl, value => model.CoverImageUrl = value);
        }

        private async Task ApplyCoverImageAsync(EditEventViewModel model)
        {
            if (model.CoverPhotoUpload?.Length > 0)
            {
                await UploadCoverPhotoAsync(model.CoverPhotoUpload, id => model.CoverPhotoId = id);
                model.CoverImageUrl = null;
                return;
            }

            NormalizeCoverImageUrl(model.CoverImageUrl, value => model.CoverImageUrl = value);
        }

        private void ValidateCoverImageUrl(IFormFile? upload, string? value)
        {
            if (upload?.Length > 0)
            {
                return;
            }

            if (!EventCoverImageResolver.IsValidExternalUrl(value))
            {
                ModelState.AddModelError("CoverImageUrl", messagesLocalizer["Messages.Event.CoverUrlInvalid"]);
            }
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

        private void NormalizeCoverImageUrl(string? value, Action<string?> setUrl)
        {
            setUrl(string.IsNullOrWhiteSpace(value) ? null : value.Trim());
        }
    }
}
