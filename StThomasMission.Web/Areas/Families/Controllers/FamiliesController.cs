using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Families.Models;

namespace StThomasMission.Web.Areas.Families.Controllers
{
    [Area("Families")]
    [Authorize(Roles = "ParishAdmin, ParishPriest")]
    public class FamiliesController : Controller
    {
        private readonly IFamilyService _familyService;
        private readonly IUnitOfWork _unitOfWork;

        public FamiliesController(IFamilyService familyService, IUnitOfWork unitOfWork)
        {
            _familyService = familyService;
            _unitOfWork = unitOfWork;
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
                // Ensure ChurchRegistrationNumber or TemporaryId is set based on IsRegistered
                string? churchRegNumber = model.IsRegistered ? (model.ChurchRegistrationNumber ?? $"10802{new Random().Next(1000, 9999)}") : null;
                string? tempId = model.IsRegistered ? null : (model.TemporaryId ?? $"TMP-{new Random().Next(1000, 9999)}");

                var family = await _familyService.RegisterFamilyAsync(
                    model.FamilyName,
                    model.Ward,
                    model.IsRegistered,
                    churchRegNumber,
                    tempId);
                return RedirectToAction(nameof(Success), new { familyId = family.Id });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Success(int familyId)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null) return NotFound();
            return View(family);
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