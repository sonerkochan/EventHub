using EventHub.Core.Contracts;
using EventHub.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EventHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEventService eventService;

        public HomeController(IEventService _eventService)
        {
            eventService = _eventService;
        }

        public async Task<IActionResult> Index()
        {
            // Get upcoming published events for the landing page
            var upcomingEvents = await eventService.GetPublishedEventsAsync();
            return View(upcomingEvents);
        }

        public IActionResult Dashboard()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            if (User.IsInRole("Client"))
            {
                return RedirectToAction("Index", "Home", new { area = "Client" });
            }

            if (User.IsInRole("Moderator"))
            {
                return RedirectToAction("Index", "Home", new { area = "Moderator" });
            }

            if (User.IsInRole("Organizer"))
            {
                return RedirectToAction("Index", "Home", new { area = "Organizer" });
            }

            if (User.IsInRole("Supplier"))
            {
                return RedirectToAction("Index", "Home", new { area = "Supplier" });
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
