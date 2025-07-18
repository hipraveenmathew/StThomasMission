using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using StThomasMission.Core.Constants;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Admin.Models;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class ImportController : Controller
    {
        private readonly IImportService _importService;
        private readonly ILogger<ImportController> _logger;

        public ImportController(IImportService importService, ILogger<ImportController> logger)
        {
            _importService = importService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ImportViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ImportViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("File", "Please select a valid Excel file to upload.");
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                var result = await _importService.ImportDataAsync(model.File.OpenReadStream(), userId);

                if (result.Success)
                {
                    TempData["Success"] = $"{result.SuccessfullyImported} records were imported successfully from {result.TotalRows} total rows.";
                    return RedirectToAction(nameof(Index));
                }

                // If not successful, populate the model with the failure details and return the view
                model.Result = result;
                ModelState.AddModelError("", $"Import failed with {result.FailedRows.Count} error(s). Please review the details below.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during file import.");
                ModelState.AddModelError("", "An unexpected error occurred. The file may be corrupt or in the wrong format.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Data");

            // Define headers for the template file
            worksheet.Cells["A1"].Value = "FamilyName";
            worksheet.Cells["B1"].Value = "WardName";
            worksheet.Cells["C1"].Value = "ChurchRegistrationNumber";
            worksheet.Cells["D1"].Value = "MemberName";

            worksheet.Cells["A1:D1"].Style.Font.Bold = true;
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var stream = new MemoryStream(package.GetAsByteArray());
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StThomasMission_Import_Template.xlsx");
        }
    }
}