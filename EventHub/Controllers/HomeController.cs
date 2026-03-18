using EventHub.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EventHub.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            if (User.IsInRole("Client"))
            {
                return RedirectToAction("Index", "Home", new { area = "Client" });
            }

            return View();
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
