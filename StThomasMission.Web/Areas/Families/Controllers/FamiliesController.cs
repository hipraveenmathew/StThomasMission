using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Families.Models;

namespace StThomasMission.Web.Areas.Families.Controllers
{
    [Area("Families")]
    [Authorize(Roles = "ParishAdmin")]
    public class FamiliesController : Controller
    {
        private readonly IFamilyService _familyService;

        public FamiliesController(IFamilyService familyService)
        {
            _familyService = familyService;
        }

        public async Task<IActionResult> Index()
        {
            var families = await _familyService.GetAllFamiliesAsync();
            return View(families);
        }

        public IActionResult Register()
        {
            return View(new FamilyViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(FamilyViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _familyService.RegisterFamilyAsync(
                    model.FamilyName,
                    model.Ward,
                    model.IsRegistered,
                    model.ChurchRegistrationNumber,
                    model.TemporaryId);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public IActionResult AddMember(int familyId)
        {
            return View(new FamilyMemberViewModel { FamilyId = familyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(FamilyMemberViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _familyService.AddFamilyMemberAsync(
                    model.FamilyId,
                    model.FirstName,
                    model.LastName,
                    model.Relation,
                    model.DateOfBirth,
                    model.Contact,
                    model.Email);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public IActionResult EnrollStudent(int familyMemberId)
        {
            return View(new StudentEnrollmentViewModel { FamilyMemberId = familyMemberId, AcademicYear = DateTime.Now.Year });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudent(StudentEnrollmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _familyService.EnrollStudentAsync(
                    model.FamilyMemberId,
                    model.Grade,
                    model.AcademicYear,
                    model.Group,
                    model.StudentOrganisation);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}