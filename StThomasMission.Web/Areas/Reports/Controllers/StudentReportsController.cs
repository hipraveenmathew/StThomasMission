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
    public class StudentReportsController : Controller
    {
        private readonly IReportingService _reportingService;

        public StudentReportsController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpGet]
        public async Task<IActionResult> StudentReport(int studentId, string format = "pdf")
        {
            if (studentId <= 0)
            {
                TempData["Error"] = "Invalid student ID.";
                return RedirectToAction("Index", "Reports");
            }

            if (!IsSupportedFormat(format))
            {
                format = "pdf"; // Default to PDF if format is unsupported
            }

            try
            {
                var reportFormat = format.ToLower() == "pdf" ? ReportFormat.Pdf : ReportFormat.Excel;
                var report = await _reportingService.GenerateStudentReportAsync(studentId, reportFormat);
                string contentType = GetContentType(format);
                string fileName = $"StudentReport_{studentId}.{GetFileExtension(format)}";

                return File(report, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to generate student report: {ex.Message}";
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