using EventHub.Core.Contracts;
using EventHub.Core.Models.Event;
using EventHub.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace EventHub.Areas.Organizer.Controllers
{
    public class EventsController : BaseController
    {
        private readonly IEventService eventService;
        private readonly IRoomService roomService;
        public EventsController(IEventService _eventService, IRoomService _roomService)
        {
            eventService = _eventService;
            roomService = _roomService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await eventService.GetAllEventsAsync();
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
            TempData["Success"] = "Event published successfully!";
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
    }
}
