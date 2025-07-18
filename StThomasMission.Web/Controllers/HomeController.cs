using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // Add this using statement
using Microsoft.Extensions.Logging;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Models;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StThomasMission.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMassTimingService _massTimingService;
        private readonly IAnnouncementService _announcementService;
        private readonly IImportService _importService;
        private readonly IConfiguration _configuration; // <-- Add this field

        public HomeController(
            ILogger<HomeController> logger,
            IMassTimingService massTimingService,
            IAnnouncementService announcementService,
            IImportService importService,
            IConfiguration configuration) // <-- Inject IConfiguration here
        {
            _logger = logger;
            _massTimingService = massTimingService;
            _announcementService = announcementService;
            _importService = importService;
            _configuration = configuration; // <-- Store the injected configuration
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                if (User.IsInRole(UserRoles.Admin))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                if (User.IsInRole(UserRoles.ParishPriest) || User.IsInRole(UserRoles.ParishAdmin))
                    return RedirectToAction("Index", "Families", new { area = "Church" });
                if (User.IsInRole(UserRoles.HeadTeacher) || User.IsInRole(UserRoles.Teacher))
                    return RedirectToAction("Index", "Students", new { area = "Catechism" });
                if (User.IsInRole(UserRoles.Parent))
                    return RedirectToAction("Index", "Portal", new { area = "Parents" });
            }

            var upcomingMasses = await _massTimingService.GetCurrentAndUpcomingMassesAsync();
            var activeAnnouncements = await _announcementService.GetActiveAnnouncementsAsync();

            var model = new HomeViewModel
            {
                MassTimings = upcomingMasses.ToList(),
                Announcements = activeAnnouncements.Select(a => new AnnouncementViewModel
                {
                    Title = a.Title,
                    Description = a.Description,
                    PostedDateFormatted = a.PostedDate.ToString("dd MMMM yyyy"),
                    AuthorName = a.AuthorName
                }).ToList()
            };

            return View(model);
        }

        public IActionResult Contact()
        {
            // This now works because _configuration is available.
            var model = new ContactViewModel
            {
                AddressLine1 = _configuration["ParishInfo:AddressLine1"],
                City = _configuration["ParishInfo:City"],
                PostCode = _configuration["ParishInfo:PostCode"],
                PhoneNumber = _configuration["ParishInfo:PhoneNumber"],
                EmailAddress = _configuration["ParishInfo:EmailAddress"],
                GoogleMapsEmbedUrl = _configuration["ParishInfo:GoogleMapsEmbedUrl"]
            };
            return View(model);
        }

        // This action would typically be in an Admin area controller
        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "No file was selected for upload.";
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _importService.ImportDataAsync(file.OpenReadStream(), userId);

            if (result.Success)
            {
                TempData["Success"] = $"{result.SuccessfullyImported} records were successfully imported.";
            }
            else
            {
                TempData["Error"] = $"Import failed with {result.FailedRows.Count} errors. Please correct the file and try again.";
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
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