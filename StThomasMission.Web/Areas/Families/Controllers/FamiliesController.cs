using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Families.Models;
using StThomasMission.Web.Models;
using System.Linq;

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

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.WardSortParm = sortOrder == "ward" ? "ward_desc" : "ward";

            var query = (await _unitOfWork.Families.GetAllAsync()).AsQueryable();


            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(f =>
                    f.FamilyName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    f.Ward.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(f => f.FamilyName),
                "ward" => query.OrderBy(f => f.Ward),
                "ward_desc" => query.OrderByDescending(f => f.Ward),
                _ => query.OrderBy(f => f.FamilyName),
            };

            int totalItems = await query.CountAsync();
            var pagedFamilies = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var model = new PaginatedList<Family>(pagedFamilies, totalItems, pageNumber, pageSize);
            ViewBag.SearchString = searchString;
            return View(model);
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
                string? churchRegNumber = model.IsRegistered
                    ? (model.ChurchRegistrationNumber ?? GenerateChurchRegistrationNumber())
                    : null;
                string? tempId = !model.IsRegistered
                    ? (model.TemporaryId ?? GenerateTemporaryId())
                    : null;

                // TODO: Check for duplicate ChurchRegistrationNumber or TemporaryId here

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
                    model.Email,
                     model.Role);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public IActionResult EnrollStudent(int familyMemberId)
        {
            return View(new StudentEnrollmentViewModel
            {
                FamilyMemberId = familyMemberId,
                AcademicYear = DateTime.Now.Year
            });
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

        [HttpGet]
        public async Task<IActionResult> ConvertToRegistered(int familyId)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null) return NotFound();
            return View(family);
        }

        [HttpPost]
        public async Task<IActionResult> ConvertToRegistered(int familyId, string churchRegistrationNumber)
        {
            try
            {
                await _familyService.ConvertTemporaryIdToChurchIdAsync(familyId, churchRegistrationNumber);
                TempData["Success"] = "Family converted to registered status successfully!";
                return RedirectToAction("Success", new { familyId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var family = await _familyService.GetFamilyByIdAsync(familyId);
                return View(family);
            }
        }

        [HttpGet]
        public async Task<IActionResult> MarkAsMigrated(int familyId)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null) return NotFound();
            return View(family);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsMigrated(int familyId, string migratedTo)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null) return NotFound();

            family.Status = FamilyStatus.Migrated; // 
            family.MigratedTo = migratedTo;

            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();

            TempData["Success"] = "Family marked as migrated successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(id);

            if (family == null) return NotFound();
            return View(family);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string familyName, string ward, bool isRegistered, string? churchRegistrationNumber, string? temporaryId, string status, string? migratedTo)
        {
            try
            {
                await _familyService.UpdateFamilyAsync(id, familyName, ward, isRegistered, churchRegistrationNumber, temporaryId, status, migratedTo);
                TempData["Success"] = "Family updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var family = await _familyService.GetFamilyByIdAsync(id);
                return View(family);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(id);

            if (family == null) return NotFound();

            family.Members = (await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(id)).ToList();
            return View(family);
        }

        #region Helpers

        private string GenerateChurchRegistrationNumber()
        {
            return $"10802{new Random().Next(1000, 9999)}";
        }

        private string GenerateTemporaryId()
        {
            return $"TMP-{new Random().Next(1000, 9999)}";
        }

        #endregion
    }
}
