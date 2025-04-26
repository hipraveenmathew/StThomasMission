using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Catechism.Models;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Catechism.Controllers
{
    [Area("Catechism")]
    [Authorize(Roles = "HeadTeacher")]
    public class HeadTeacherController : Controller
    {
        private readonly ICatechismService _catechismService;

        public HeadTeacherController(ICatechismService catechismService)
        {
            _catechismService = catechismService;
        }

        [HttpGet]
        public IActionResult PromoteStudents()
        {
            return View(new PromoteStudentsViewModel { AcademicYear = DateTime.UtcNow.Year });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteStudents(PromoteStudentsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _catechismService.PromoteStudentsAsync(model.Grade, model.AcademicYear);
                model.SuccessMessage = $"Students in {model.Grade} for academic year {model.AcademicYear} have been promoted successfully.";
                model.Grade = string.Empty;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Promotion failed: {ex.Message}");
            }

            return View(model);
        }
    }
}