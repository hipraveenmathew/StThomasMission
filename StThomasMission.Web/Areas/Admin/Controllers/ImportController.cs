using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ImportController : Controller
    {
        private readonly IImportService _importService;

        public ImportController(IImportService importService)
        {
            _importService = importService;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid file.");
                return View();
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var result = await _importService.ImportFamiliesAndStudentsAsync(stream);

            if (result)
            {
                TempData["Success"] = "Data imported successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to import data.";
            }

            return RedirectToAction("Upload");
        }
        [HttpGet]
        public IActionResult ImportFamilies()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportFamilies(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel file.";
                return View();
            }

            using var stream = excelFile.OpenReadStream();
            var (success, errors) = await _importService.ImportFamiliesFromExcelAsync(stream);

            if (success)
            {
                TempData["Success"] = "Families imported successfully!";
            }
            else
            {
                TempData["Error"] = "Errors occurred during import.";
                TempData["Errors"] = string.Join("<br/>", errors);
            }

            return View();
        }

        [HttpGet]
        public IActionResult ImportStudents()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportStudents(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel file.";
                return View();
            }

            using var stream = excelFile.OpenReadStream();
            var (success, errors) = await _importService.ImportStudentsFromExcelAsync(stream);

            if (success)
            {
                TempData["Success"] = "Students imported successfully!";
            }
            else
            {
                TempData["Error"] = "Errors occurred during import.";
                TempData["Errors"] = string.Join("<br/>", errors);
            }

            return View();
        }
    }
}