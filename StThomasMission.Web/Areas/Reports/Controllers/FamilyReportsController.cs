using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Reports.Controllers
{
    [Area("Reports")]
    [Authorize(Roles = "ParishAdmin")]
    public class FamilyReportsController : Controller
    {
        private readonly IReportingService _reportingService;

        public FamilyReportsController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpGet]
        public async Task<IActionResult> FamilyReport(string ward, string status, string format = "pdf")
        {
            int? wardId = null;
            if (!string.IsNullOrEmpty(ward) && int.TryParse(ward, out int parsedWardId))
            {
                wardId = parsedWardId;
            }

            FamilyStatus? familyStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<FamilyStatus>(status, true, out var parsedStatus))
            {
                familyStatus = parsedStatus;
            }

            if (!IsSupportedFormat(format))
            {
                format = "pdf"; // Default to PDF if format is unsupported
            }

            try
            {
                var reportFormat = format.ToLower() == "pdf" ? ReportFormat.Pdf : ReportFormat.Excel;
                var report = await _reportingService.GenerateFamilyReportAsync(wardId, familyStatus, reportFormat);
                string contentType = GetContentType(format);
                string fileName = $"FamilyReport.{GetFileExtension(format)}";

                return File(report, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to generate family report: {ex.Message}";
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