using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Web.Models;

namespace StThomasMission.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("An error occurred with Request ID: {RequestId}", requestId);

            var model = new ErrorViewModel
            {
                RequestId = requestId,
                Message = "An unexpected error occurred. Please try again later."
            };
            return View(model);
        }
    }
}