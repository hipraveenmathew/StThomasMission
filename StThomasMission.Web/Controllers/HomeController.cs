using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services;
using StThomasMission.Web.Models;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;

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
        public class StudentTempModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int RegistrationNumber { get; set; }  // Church Registration Number
            public string ParentName { get; set; } = string.Empty;
            public string ParentContact1 { get; set; } = string.Empty;
            public string? ParentContact2 { get; set; }  // Optional second number
            public string ParentEmail { get; set; } = string.Empty;
            public int Fee { get; set; }  // Stored as integer e.g., 20
        }

        [HttpPost]
        public IActionResult UploadStudentExcel(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                TempData["Error"] = "No file selected.";
                return RedirectToAction("About");
            }

            var studentList = new List<StudentTempModel>();

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        TempData["Error"] = "No worksheet found in Excel.";
                        return RedirectToAction("About");
                    }

                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++) // Assuming row 1 is header
                    {
                        var phoneCell = worksheet.Cells[row, 5].Text.Trim();
                        var phones = phoneCell.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                              .Select(p => p.Trim()).ToArray();

                        var student = new StudentTempModel
                        {
                            Id = Convert.ToInt32(worksheet.Cells[row, 1].Text.Trim()),
                            Name = worksheet.Cells[row, 2].Text.Trim(),
                            RegistrationNumber = Convert.ToInt32(worksheet.Cells[row, 3].Text.Trim()),
                            ParentName = worksheet.Cells[row, 4].Text.Trim(),
                            ParentContact1 = phones.Length > 0 ? phones[0] : "",
                            ParentContact2 = phones.Length > 1 ? phones[1] : null,
                            ParentEmail = worksheet.Cells[row, 6].Text.Trim(),
                            Fee = ParseFeeToInt(worksheet.Cells[row, 7].Text.Trim())
                        };

                        studentList.Add(student);
                    }
                }
            }

            TempData["Success"] = $"{studentList.Count} student records uploaded successfully!";
            return RedirectToAction("About");
        }

        private int ParseFeeToInt(string feeText)
        {
            if (string.IsNullOrWhiteSpace(feeText))
                return 0;

            // Remove any currency symbols and unnecessary characters
            feeText = feeText.Replace("$", "").Replace("£", "").Replace(",", "").Trim();

            if (decimal.TryParse(feeText, out var feeDecimal))
            {
                return Convert.ToInt32(Math.Round(feeDecimal));  // Convert 20.00 -> 20
            }

            return 0;
        }

    }
}