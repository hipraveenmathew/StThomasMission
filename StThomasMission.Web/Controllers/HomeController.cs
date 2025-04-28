using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Models;

namespace StThomasMission.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMassTimingService _massTimingService;
        private readonly IAnnouncementService _announcementService;

        public HomeController(IMassTimingService massTimingService, IAnnouncementService announcementService,ILogger<HomeController> logger)
        {
            _logger = logger;
            _massTimingService = massTimingService;
            _announcementService = announcementService;
        }

        public async Task<IActionResult> Index()
        {
            var massTimings = await _massTimingService.GetMassTimingsAsync();
            var announcements = await _announcementService.GetAnnouncementsAsync();

            var model = new HomeIndexViewModel
            {
                MassTimings = massTimings,
                Announcements = announcements
            };

            return View(model);
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