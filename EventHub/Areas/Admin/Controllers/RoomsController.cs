using EventHub.Core.Contracts;
using EventHub.Core.Models.Room;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace EventHub.Areas.Admin.Controllers
{
    public class RoomsController : BaseController
    {
        private readonly IRoomService roomService;
        private readonly IVenueService venueService;
        private readonly ISeatLayoutService seatLayoutService;

        public RoomsController(
            IRoomService _roomService,
            IVenueService _venueService,
            ISeatLayoutService _seatLayoutService)
        {
            roomService = _roomService;
            venueService = _venueService;
            seatLayoutService = _seatLayoutService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await roomService.GetAllRoomsAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreatePartial()
        {
            var model = new AddRoomViewModel();
            ViewBag.Venues = await BuildVenueSelectList();
            return PartialView("_CreateModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Venues = await BuildVenueSelectList();
                return PartialView("_CreateModal", model);
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await roomService.AddRoomAsync(model, userId);

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> EditPartial(Guid id)
        {
            var model = await roomService.GetRoomForEditAsync(id);
            if (model == null) return NotFound();

            ViewBag.Venues = await BuildVenueSelectList();
            return PartialView("_EditModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Venues = await BuildVenueSelectList();
                return PartialView("_EditModal", model);
            }

            var success = await roomService.UpdateRoomAsync(model);
            if (!success) return NotFound();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            await roomService.DeactivateRoomAsync(id);
            return Json(new { success = true });
        }

        // ── Seat Layout Editor ──

        [HttpGet]
        public async Task<IActionResult> Layout(Guid id)
        {
            var model = await seatLayoutService.GetLayoutEditorDataAsync(id);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveLayout([FromBody] SaveSeatLayoutRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await seatLayoutService.SaveLayoutAsync(request, userId);

            var data = await seatLayoutService.GetLayoutEditorDataAsync(request.RoomId);
            return Json(new { success = true, seats = data.Seats, zones = data.Zones });
        }

        [HttpPost]
        public async Task<IActionResult> CreateZone([FromBody] CreateZoneRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var zone = await seatLayoutService.CreateZoneAsync(request, userId);
            return Json(new { success = true, zone });
        }

        [HttpPost]
        public async Task<IActionResult> AssignZone([FromBody] AssignZoneRequest request)
        {
            await seatLayoutService.AssignSeatsToZoneAsync(request);

            var data = await seatLayoutService.GetLayoutEditorDataAsync(request.RoomId);
            return Json(new { success = true, seats = data.Seats, zones = data.Zones });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromZone([FromBody] RemoveFromZoneRequest request)
        {
            await seatLayoutService.RemoveSeatsFromZoneAsync(request);

            var data = await seatLayoutService.GetLayoutEditorDataAsync(request.RoomId);
            return Json(new { success = true, seats = data.Seats, zones = data.Zones });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteZone([FromBody] DeleteZoneRequest request)
        {
            await seatLayoutService.DeleteZoneAsync(request.Id);
            return Json(new { success = true });
        }

        private async Task<IEnumerable<SelectListItem>> BuildVenueSelectList()
        {
            var venues = await venueService.GetAllVenuesAsync();
            return venues.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.Name} ({v.City})"
            });
        }
    }
}
