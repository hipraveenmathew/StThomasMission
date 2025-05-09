using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services;
using StThomasMission.Web.Models;

namespace StThomasMission.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMassTimingService _massTimingService;
        private readonly IAnnouncementService _announcementService;

        public HomeController(IMassTimingService massTimingService, IAnnouncementService announcementService, ILogger<HomeController> logger)
        {
            _logger = logger;
            _massTimingService = massTimingService;
            _announcementService = announcementService;
        }

        public async Task<IActionResult> Index()
        {
            // Check if the user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                // Redirect based on user role
                if (User.IsInRole("ParishPriest") || User.IsInRole("ParishAdmin"))
                {
                    return RedirectToAction("Index", "Families", new { area = "Families" });
                }
                else if (User.IsInRole("HeadTeacher") || User.IsInRole("Teacher"))
                {
                    return RedirectToAction("Index", "Students", new { area = "Catechism" });
                }
                else if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin", new { area = "Admin" });
                }
                else if (User.IsInRole("Parent"))
                {
                    return RedirectToAction("Index", "Portal", new { area = "Parents" });
                }
            }

            // If user is not authenticated or has an unrecognized role, show the public homepage
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
        public IActionResult About()
        {
            return View();
        }

        public async Task<IActionResult> Events()
        {
            try
            {
                var announcements = await _announcementService.GetAnnouncementsAsync();
                var activeAnnouncements = announcements
                    .Where(a => a.IsActive && !a.IsDeleted)
                    .OrderByDescending(a => a.PostedDate)
                    .ToList();

                var model = new EventsViewModel
                {
                    Announcements = activeAnnouncements
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load announcements: {ex.Message}";
                return View(new EventsViewModel());
            }
        }

        public IActionResult Contact()
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