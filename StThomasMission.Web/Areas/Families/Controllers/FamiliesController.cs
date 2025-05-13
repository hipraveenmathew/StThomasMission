using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure;
using StThomasMission.Services;
using StThomasMission.Web.Areas.Families.Models;
using StThomasMission.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Families.Controllers
{
    [Area("Families")]
    [Authorize(Roles = "ParishAdmin,ParishPriest")]
    public class FamiliesController : Controller
    {
        private readonly IFamilyService _familyService;
        private readonly IFamilyMemberService _familyMemberService;
        private readonly IUnitOfWork _unitOfWork;

        public FamiliesController(IFamilyService familyService, IUnitOfWork unitOfWork, IFamilyMemberService familyMemberService)
        {
            _familyService = familyService;
            _unitOfWork = unitOfWork;
            _familyMemberService = familyMemberService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(FamilyFilterViewModel filter, string sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["WardSortParm"] = sortOrder == "ward" ? "ward_desc" : "ward";

            // Build the query with server-side filtering
            var query = _familyService.GetFamiliesQueryable(
                searchString: filter.SearchString,
                ward: filter.Ward,
                status: filter.Status
            );

            // Apply sorting
            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(f => f.FamilyName),
                "ward" => query.OrderBy(f => f.Ward.Name),
                "ward_desc" => query.OrderByDescending(f => f.Ward.Name),
                _ => query.OrderBy(f => f.FamilyName),
            };

            // Pagination
            int totalItems = await query.CountAsync();
            var pagedFamilies = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var model = new PaginatedList<Family>(pagedFamilies, totalItems, pageNumber, pageSize);
            return View(new FamilyIndexViewModel
            {
                Filter = filter,
                Families = model
            });
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new FamilyViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(FamilyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string? churchRegNumber = model.IsRegistered
                    ? (model.ChurchRegistrationNumber ?? await GenerateUniqueChurchRegistrationNumber())
                    : null;
                string? tempId = !model.IsRegistered
                    ? (model.TemporaryID ?? await GenerateUniqueTemporaryId())
                    : null;

                var family = new Family
                {
                    FamilyName = model.FamilyName,
                    WardId = int.Parse(model.Ward),
                    IsRegistered = model.IsRegistered,
                    ChurchRegistrationNumber = churchRegNumber,
                    TemporaryID = tempId,
                    CreatedBy = User.Identity?.Name ?? "System",
                    CreatedDate = DateTime.UtcNow
                };

                await _familyService.RegisterFamilyAsync(family);
                return RedirectToAction(nameof(Success), new { familyId = family.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to register family: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success(int familyId)
        {
            var family = await _familyService.GetFamilyByIdAsync(familyId);
            if (family == null)
            {
                return NotFound("Family not found.");
            }

            var model = new FamilyViewModel
            {
                Id = family.Id,
                FamilyName = family.FamilyName,
                Ward = family.WardId.ToString(),
                IsRegistered = family.IsRegistered,
                ChurchRegistrationNumber = family.ChurchRegistrationNumber,
                TemporaryID = family.TemporaryID,
                Status = family.Status,
                MigratedTo = family.MigratedTo
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult AddMember(int familyId)
        {
            return View(new FamilyMemberViewModel { FamilyId = familyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(FamilyMemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var familyMember = new FamilyMember
                {
                    FamilyId = model.FamilyId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Relation = model.Relation,
                    DateOfBirth = model.DateOfBirth,
                    Contact = model.Contact,
                    Email = model.Email,
                    Role = model.Role,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                await _familyService.AddFamilyMemberAsync(familyMember);
                return RedirectToAction(nameof(Details), new { id = model.FamilyId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to add family member: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _familyService.EnrollStudentAsync(
                    model.FamilyMemberId,
                    model.Grade,
                    model.AcademicYear,
                    model.Group,
                    model.StudentOrganisation);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to enroll student: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConvertToRegistered(int familyId)
        {
            var family = await _familyService.GetFamilyByIdAsync(familyId);
            if (family == null)
            {
                return NotFound("Family not found.");
            }

            try
            {
                await _familyService.ConvertTemporaryIdToChurchIdAsync(familyId);
                return RedirectToAction(nameof(Success), new { familyId = familyId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to convert family: {ex.Message}");
                return View();
            }
            
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ConvertToRegistered(ConvertToRegisteredViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    try
        //    {
        //        await _familyService.ConvertTemporaryIdToChurchIdAsync(model.FamilyId);
        //        return RedirectToAction(nameof(Success), new { familyId = model.FamilyId });
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError(string.Empty, $"Failed to convert family: {ex.Message}");
        //        return View(model);
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> MarkAsMigrated(int id)
        {
            var family = await _familyService.GetFamilyByIdAsync(id);
            if (family == null)
            {
                return NotFound("Family not found.");
            }

            var model = new MarkAsMigratedViewModel
            {
                Id = family.Id,
                FamilyName = family.FamilyName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsMigrated(MarkAsMigratedViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var family = new Family
                {
                    Id = model.Id,
                    FamilyName = model.FamilyName,
                    WardId = int.Parse(model.Ward),
                    IsRegistered = model.IsRegistered,
                    ChurchRegistrationNumber = model.ChurchRegistrationNumber,
                    TemporaryID = model.TemporaryID,
                    Status = FamilyStatus.Migrated,
                    MigratedTo = model.MigratedTo,
                    UpdatedBy = User.Identity?.Name ?? "System",
                    UpdatedDate = DateTime.UtcNow
                };

                await _familyService.UpdateFamilyAsync(family);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to mark family as migrated: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var family = await _familyService.GetFamilyByIdAsync(id);
            if (family == null)
            {
                return NotFound("Family not found.");
            }

            var model = new FamilyViewModel
            {
                Id = family.Id,
                FamilyName = family.FamilyName,
                Ward = family.WardId.ToString(),
                IsRegistered = family.IsRegistered,
                ChurchRegistrationNumber = family.ChurchRegistrationNumber,
                TemporaryID = family.TemporaryID,
                Status = family.Status,
                MigratedTo = family.MigratedTo
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FamilyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var family = new Family
                {
                    Id = model.Id,
                    FamilyName = model.FamilyName,
                    WardId = int.Parse(model.Ward),
                    IsRegistered = model.IsRegistered,
                    ChurchRegistrationNumber = model.ChurchRegistrationNumber,
                    TemporaryID = model.TemporaryID,
                    Status = model.Status,
                    MigratedTo = model.MigratedTo,
                    UpdatedBy = User.Identity?.Name ?? "System",
                    UpdatedDate = DateTime.UtcNow
                };

                await _familyService.UpdateFamilyAsync(family);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to update family: {ex.Message}");
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var family = await _familyService.GetFamilyByIdAsync(id);
            if (family == null)
            {
                return NotFound("Family not found.");
            }

            var members = await _familyService.GetFamilyMembersByFamilyIdAsync(id);
            var model = new FamilyDetailsViewModel
            {
                Id = family.Id,
                FamilyName = family.FamilyName,
                Ward = family.WardId.ToString(),
                IsRegistered = family.IsRegistered,
                ChurchRegistrationNumber = family.ChurchRegistrationNumber,
                TemporaryID = family.TemporaryID,
                Status = family.Status,
                MigratedTo = family.MigratedTo,
                PreviousFamilyId = family.PreviousFamilyId,
                PreviousFamily = family.PreviousFamily,
                Members = members.Select(m => new FamilyMemberViewModel
                {
                    Id = m.Id,
                    FamilyId = m.FamilyId,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    Relation = m.Relation,
                    DateOfBirth = m.DateOfBirth,
                    Contact = m.Contact,
                    Email = m.Email,
                    Role = m.Role
                }).ToList()
            };

            return View(model);
        }

        private async Task<string> GenerateUniqueChurchRegistrationNumber()
        {
            
            string number = await _familyService.NewChurchIdAsync();            
            return number;
        }

        private async Task<string> GenerateUniqueTemporaryId()
        {
            string id;
            do
            {
                id = $"TMP-{new Random().Next(1000, 9999)}";
            } while (await _familyService.GetByTemporaryIdAsync(id) != null);
            return id;
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
    }
}