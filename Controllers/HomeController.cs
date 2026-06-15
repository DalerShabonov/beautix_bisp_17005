using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using beautix_bisp_17005.Models;

namespace beautix_bisp_17005.Controllers
{
    /// <summary>
    /// Public, no-login pages: the landing page, the privacy page and the shared
    /// error page. This is the default route ("/"), so it's the first thing a
    /// visitor sees. It deliberately touches no database, which is why the site can
    /// still serve this page even if the DB is down (see Program.cs startup).
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // The marketing/landing page.
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Friendly error page. NoStore/Duration 0 stops it being cached so users
        // never see a stale error. RequestId helps correlate with server logs.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}