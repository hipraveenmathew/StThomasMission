using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using System;
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

        [HttpGet]
        public async Task<IActionResult> StudentReport(int studentId, string format = "pdf")
        {
            var fileContent = await _reportingService.GenerateStudentReportAsync(studentId, format);
            string contentType = format == "pdf" ? "application/pdf" :
                                 "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"StudentReport_{studentId}.{(format == "pdf" ? "pdf" : "xlsx")}";

            return File(fileContent, contentType, fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ClassReport(string grade, int academicYear, string format = "pdf")
        {
            var fileContent = await _reportingService.GenerateClassReportAsync(grade, academicYear, format);
            string contentType = format == "pdf" ? "application/pdf" :
                                 "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"ClassReport_{grade}_{academicYear}.{(format == "pdf" ? "pdf" : "xlsx")}";

            return File(fileContent, contentType, fileName);
        }

        [HttpGet]
        public async Task<IActionResult> OverallCatechismReport(int academicYear, string format = "pdf")
        {
            var fileContent = await _reportingService.GenerateCatechismReportAsync(academicYear, format);
            string contentType = format == "pdf" ? "application/pdf" :
                                 "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"OverallCatechismReport_{academicYear}.{(format == "pdf" ? "pdf" : "xlsx")}";

            return File(fileContent, contentType, fileName);
        }
    }
}
