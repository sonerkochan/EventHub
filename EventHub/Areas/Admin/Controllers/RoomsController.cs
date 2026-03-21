using EventHub.Core.Contracts;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Areas.Admin.Controllers
{
    public class RoomsController : BaseController
    {
        private readonly IRoomService roomService;

        public RoomsController(IRoomService _roomService)
        {
            roomService = _roomService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new Room();
                //await roomService.GetSingleRoomById(Guid.Parse("156EC0AE-ED2D-42C2-9FD1-CD4C1E224579"));// NO HARDCODED RANDOM GUIDS!
            return View(model);
        }
    }
}
