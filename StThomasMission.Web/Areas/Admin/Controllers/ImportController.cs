using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using System.IO;
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

            try
            {
                await _importService.ImportFamiliesAndStudentsAsync(stream, "Excel");
                TempData["Success"] = "Data imported successfully!";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Import failed: {ex.Message}";
            }

            return RedirectToAction("Upload");
        }
    }
}
