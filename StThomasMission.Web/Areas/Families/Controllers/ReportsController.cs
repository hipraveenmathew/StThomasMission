using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Families.Controllers
{
    [Area("Families")]
    [Authorize(Roles = "ParishAdmin, ParishPriest")]
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

        public async Task<IActionResult> FamilyReport(string format = "pdf")
        {
            byte[] fileContent;
            string contentType;
            string fileName;

            if (format.ToLower() == "pdf")
            {
                fileContent = await _reportingService.GenerateFamilyReportPdfAsync();
                contentType = "application/pdf";
                fileName = "FamilyReport.pdf";
            }
            else
            {
                fileContent = await _reportingService.GenerateFamilyReportExcelAsync();
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = "FamilyReport.xlsx";
            }

            return File(fileContent, contentType, fileName);
        }
    }
}