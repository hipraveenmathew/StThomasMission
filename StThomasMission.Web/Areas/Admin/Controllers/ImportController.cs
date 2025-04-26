using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Admin.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ImportController : Controller
    {
        private readonly IImportService _importService;

        public ImportController(IImportService importService)
        {
            _importService = importService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ImportViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(ImportViewModel model)
        {
            if (!ModelState.IsValid || model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("File", "Please upload a valid Excel file.");
                return View("Index", model);
            }

            try
            {
                using var stream = new MemoryStream();
                await model.File.CopyToAsync(stream);
                await _importService.ImportFamiliesAndStudentsAsync(stream, ImportType.Excel); // Fixed the second argument to use the correct enum
                model.SuccessMessage = "Data imported successfully!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("File", $"Import failed: {ex.Message}");
            }

            return View("Index", model);
        }
    }
}