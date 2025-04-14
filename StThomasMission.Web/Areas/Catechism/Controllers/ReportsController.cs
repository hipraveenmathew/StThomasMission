using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Catechism.Controllers
{
    [Area("Catechism")]
    [Authorize(Roles = "HeadTeacher, ParishPriest")]
    public class ReportsController : Controller
    {
        private readonly IReportingService _reportingService;

        public ReportsController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> StudentReport(int studentId, string format = "pdf")
        {
            byte[] fileContent;
            string contentType;
            string fileName;

            if (format.ToLower() == "pdf")
            {
                fileContent = await _reportingService.GenerateStudentReportPdfAsync(studentId);
                contentType = "application/pdf";
                fileName = $"StudentReport_{studentId}.pdf";
            }
            else
            {
                fileContent = await _reportingService.GenerateStudentReportExcelAsync(studentId);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"StudentReport_{studentId}.xlsx";
            }

            return File(fileContent, contentType, fileName);
        }

        public async Task<IActionResult> ClassReport(string grade, string format = "pdf")
        {
            byte[] fileContent;
            string contentType;
            string fileName;

            if (format.ToLower() == "pdf")
            {
                fileContent = await _reportingService.GenerateClassReportPdfAsync(grade);
                contentType = "application/pdf";
                fileName = $"ClassReport_{grade}.pdf";
            }
            else
            {
                fileContent = await _reportingService.GenerateClassReportExcelAsync(grade);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"ClassReport_{grade}.xlsx";
            }

            return File(fileContent, contentType, fileName);
        }

        public async Task<IActionResult> OverallCatechismReport(string format = "pdf")
        {
            byte[] fileContent;
            string contentType;
            string fileName;

            if (format.ToLower() == "pdf")
            {
                fileContent = await _reportingService.GenerateOverallCatechismReportPdfAsync();
                contentType = "application/pdf";
                fileName = "OverallCatechismReport.pdf";
            }
            else
            {
                fileContent = await _reportingService.GenerateOverallCatechismReportExcelAsync();
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = "OverallCatechismReport.xlsx";
            }

            return File(fileContent, contentType, fileName);
        }
    }
}