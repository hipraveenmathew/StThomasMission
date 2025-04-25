using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Reports.Controllers
{
    [Area("Reports")]
    [Authorize(Roles = "HeadTeacher,ParishAdmin")]
    public class ReportsController : Controller
    {
        private readonly IReportingService _reportingService;

        public ReportsController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> StudentReport(int studentId, string format = "pdf")
        {
            var report = await _reportingService.GenerateStudentReportAsync(studentId, format.ToLower());
            string contentType = GetContentType(format);
            string fileName = $"StudentReport_{studentId}.{GetFileExtension(format)}";

            return File(report, contentType, fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ClassReport(string grade, int academicYear, string format = "pdf")
        {
            var report = await _reportingService.GenerateClassReportAsync(grade, academicYear, format.ToLower());
            string contentType = GetContentType(format);
            string fileName = $"ClassReport_{grade}_{academicYear}.{GetFileExtension(format)}";

            return File(report, contentType, fileName);
        }

        [HttpGet]
        public async Task<IActionResult> OverallCatechismReport(int academicYear, string format = "pdf")
        {
            var report = await _reportingService.GenerateCatechismReportAsync(academicYear, format.ToLower());
            string contentType = GetContentType(format);
            string fileName = $"OverallCatechismReport_{academicYear}.{GetFileExtension(format)}";

            return File(report, contentType, fileName);
        }

        [HttpGet]
        public async Task<IActionResult> FamilyReport(string? ward, string? status, string format = "pdf")
        {
            var report = await _reportingService.GenerateFamilyReportAsync(ward, status, format.ToLower());
            string contentType = GetContentType(format);
            string fileName = $"FamilyReport.{GetFileExtension(format)}";

            return File(report, contentType, fileName);
        }

        // Helper methods
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
