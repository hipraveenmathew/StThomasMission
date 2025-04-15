using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Families.Models;
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

            var families = await _unitOfWork.Families.GetAllAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                families = families.Where(f => f.FamilyName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                                               f.Ward.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            families = sortOrder switch
            {
                "name_desc" => families.OrderByDescending(f => f.FamilyName).ToList(),
                "ward" => families.OrderBy(f => f.Ward).ToList(),
                "ward_desc" => families.OrderByDescending(f => f.Ward).ToList(),
                _ => families.OrderBy(f => f.FamilyName).ToList(),
            };

            int totalItems = families.Count;
            var pagedFamilies = families.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

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
        [HttpGet]
        public async Task<IActionResult> ConvertToRegistered(int familyId)
        {
            var family = await _familyService.GetByIdAsync(familyId);
            if (family == null)
            {
                return NotFound();
            }
            return View(family);
        }

        [HttpPost]
        public async Task<IActionResult> ConvertToRegistered(int familyId, string churchRegistrationNumber)
        {
            await _familyService.ConvertToRegisteredAsync(familyId, churchRegistrationNumber);
            TempData["Success"] = "Family converted to registered status successfully!";
            return RedirectToAction("Success", new { familyId });
        }
        [HttpGet]
        public async Task<IActionResult> MarkAsMigrated(int familyId)
        {
            var family = await _familyService.GetByIdAsync(familyId);
            if (family == null)
            {
                return NotFound();
            }
            return View(family);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsMigrated(int familyId, string migratedTo)
        {
            var family = await _familyService.GetByIdAsync(familyId);
            if (family == null)
            {
                return NotFound();
            }

            family.Status = "Migrated";
            family.MigratedTo = migratedTo;
            await _familyService.UpdateAsync(family);

            TempData["Success"] = "Family marked as migrated successfully!";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var family = await _familyService.GetByIdAsync(id);
            if (family == null)
            {
                return NotFound();
            }
            return View(family);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string familyName, string ward, bool isRegistered, string? churchRegistrationNumber, string? temporaryId, string status)
        {
            var family = await _familyService.GetByIdAsync(id);
            if (family == null)
            {
                return NotFound();
            }

            family.FamilyName = familyName;
            family.Ward = ward;
            family.IsRegistered = isRegistered;
            family.ChurchRegistrationNumber = isRegistered ? churchRegistrationNumber : null;
            family.TemporaryID = !isRegistered ? temporaryId : null;
            family.Status = status;

            await _familyService.UpdateAsync(family);
            TempData["Success"] = "Family updated successfully!";
            return RedirectToAction("Index");
        }

       
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var family = await _familyService.GetByIdAsync(id);
            if (family == null)
            {
                return NotFound();
            }
            family.Members = (await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(id)).ToList();
            return View(family);
        }
    }
}