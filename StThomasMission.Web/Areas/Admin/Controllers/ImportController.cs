using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services;
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
        private readonly IFamilyService _familyService;
        private readonly IFamilyMemberService _familyMemberService;
        private readonly IUnitOfWork _unitOfWork;

        public ImportController(IImportService importService, IFamilyService familyService, IUnitOfWork unitOfWork, IFamilyMemberService familyMemberService)
        {
            _importService = importService;
            _unitOfWork = unitOfWork;
            _familyService = familyService;
            _familyMemberService = familyMemberService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ImportViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ImportFamilies(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel file.";
                return RedirectToAction("Index");
            }

            try
            {
                using var stream = file.OpenReadStream();
                var importService = new FamilyImportService(_familyService, _unitOfWork, _familyMemberService);
                await importService.ImportFamiliesFromExcelAsync(stream);
                TempData["Success"] = "Families imported successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to import families: {ex.Message}";
            }

            return RedirectToAction("Index");
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