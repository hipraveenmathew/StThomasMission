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
        public async Task<IActionResult> StudentReport(int studentId, string format)
        {
            byte[] report;
            string contentType;
            string fileExtension;

            if (format.ToLower() == "pdf")
            {
                report = await _reportingService.GenerateStudentReportPdfAsync(studentId);
                contentType = "application/pdf";
                fileExtension = "pdf";
            }
            else
            {
                report = await _reportingService.GenerateStudentReportExcelAsync(studentId);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileExtension = "xlsx";
            }

            return File(report, contentType, $"StudentReport_{studentId}.{fileExtension}");
        }

        [HttpGet]
        public async Task<IActionResult> ClassReport(string grade, string format)
        {
            byte[] report;
            string contentType;
            string fileExtension;

            if (format.ToLower() == "pdf")
            {
                report = await _reportingService.GenerateClassReportPdfAsync(grade);
                contentType = "application/pdf";
                fileExtension = "pdf";
            }
            else
            {
                report = await _reportingService.GenerateClassReportExcelAsync(grade);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileExtension = "xlsx";
            }

            return File(report, contentType, $"ClassReport_{grade}.{fileExtension}");
        }

        [HttpGet]
        public async Task<IActionResult> OverallCatechismReport(string format)
        {
            byte[] report;
            string contentType;
            string fileExtension;

            if (format.ToLower() == "pdf")
            {
                report = await _reportingService.GenerateOverallCatechismReportPdfAsync();
                contentType = "application/pdf";
                fileExtension = "pdf";
            }
            else
            {
                report = await _reportingService.GenerateOverallCatechismReportExcelAsync();
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileExtension = "xlsx";
            }

            return File(report, contentType, $"OverallCatechismReport.{fileExtension}");
        }

        [HttpGet]
        public async Task<IActionResult> FamilyReport(string format)
        {
            byte[] report;
            string contentType;
            string fileExtension;

            if (format.ToLower() == "pdf")
            {
                report = await _reportingService.GenerateFamilyReportPdfAsync();
                contentType = "application/pdf";
                fileExtension = "pdf";
            }
            else
            {
                report = await _reportingService.GenerateFamilyReportExcelAsync();
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileExtension = "xlsx";
            }

            return File(report, contentType, $"FamilyReport.{fileExtension}");
        }
    }
}