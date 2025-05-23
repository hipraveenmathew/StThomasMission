﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteStudents(string grade, int academicYear)
        {
            if (string.IsNullOrWhiteSpace(grade) || academicYear <= 0)
            {
                TempData["Error"] = "Please enter a valid grade and academic year.";
                return View();
            }

          await _catechismService.PromoteStudentsAsync(grade, academicYear);

          
                TempData["Success"] = $"Students in {grade} for academic year {academicYear} have been promoted successfully.";
           
                TempData["Error"] = "Promotion failed. Please verify student data.";
            

            return RedirectToAction(nameof(PromoteStudents));
        }
    }
}
