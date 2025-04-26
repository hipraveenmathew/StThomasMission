using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Reports.Controllers
{
    [Area("Reports")]
    [Authorize(Roles = "HeadTeacher,ParishAdmin")]
    public class CatechismReportsController : Controller
    {
        private readonly IReportingService _reportingService;

        public CatechismReportsController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpGet]
        public async Task<IActionResult> OverallCatechismReport(int academicYear, string format = "pdf")
        {
            if (academicYear < 2000 || academicYear > DateTime.UtcNow.Year)
            {
                TempData["Error"] = "Invalid academic year.";
                return RedirectToAction("Index", "Reports");
            }

            if (!IsSupportedFormat(format))
            {
                format = "pdf"; // Default to PDF if format is unsupported
            }

            try
            {
                var reportFormat = format.ToLower() == "pdf" ? ReportFormat.Pdf : ReportFormat.Excel;
                var report = await _reportingService.GenerateCatechismReportAsync(academicYear, reportFormat);
                string contentType = GetContentType(format);
                string fileName = $"OverallCatechismReport_{academicYear}.{GetFileExtension(format)}";

                return File(report, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to generate overall catechism report: {ex.Message}";
                return RedirectToAction("Index", "Reports");
            }
        }

        private bool IsSupportedFormat(string format)
        {
            return format.ToLower() is "pdf" or "excel";
        }

        private string GetContentType(string format)
        {
            return format.ToLower() switch
            {
                "pdf" => "application/pdf",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
        }

        private string GetFileExtension(string format)
        {
            return format.ToLower() == "excel" ? "xlsx" : "pdf";
        }
    }
}