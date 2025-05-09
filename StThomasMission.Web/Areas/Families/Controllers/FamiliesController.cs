using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
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

        public FamiliesController(IFamilyService familyService)
        {
            _familyService = familyService;
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

                var family = await _familyService.RegisterFamilyAsync(
                    model.FamilyName,
                    int.Parse(model.Ward),
                    model.IsRegistered,
                    churchRegNumber,
                    tempId);

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
                Status = family.Status.ToString(),
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
                await _familyService.AddFamilyMemberAsync(
                    model.FamilyId,
                    model.FirstName,
                    model.LastName,
                    model.Relation,
                    model.DateOfBirth,
                    model.Contact,
                    model.Email,
                    model.Role);
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

            var model = new ConvertToRegisteredViewModel
            {
                FamilyId = family.Id,
                FamilyName = family.FamilyName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertToRegistered(ConvertToRegisteredViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _familyService.ConvertTemporaryIdToChurchIdAsync(model.FamilyId, model.ChurchRegistrationNumber);
                return RedirectToAction(nameof(Success), new { familyId = model.FamilyId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to convert family: {ex.Message}");
                return View(model);
            }
        }

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
                await _familyService.UpdateFamilyAsync(
                    model.Id,
                    model.FamilyName,
                    int.Parse(model.Ward),
                    model.IsRegistered,
                    model.ChurchRegistrationNumber,
                    model.TemporaryID,
                    FamilyStatus.Migrated,
                    model.MigratedTo);
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
                Status = family.Status.ToString(),
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
                await _familyService.UpdateFamilyAsync(
                    model.Id,
                    model.FamilyName,
                    int.Parse(model.Ward),
                    model.IsRegistered,
                    model.ChurchRegistrationNumber,
                    model.TemporaryID,
                    Enum.Parse<FamilyStatus>(model.Status), // Parse string to FamilyStatus
                    model.MigratedTo);
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
                Status = family.Status.ToString(),
                MigratedTo = family.MigratedTo,
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
            string number;
            do
            {
                number = $"10802{new Random().Next(1000, 9999)}";
            } while (await _familyService.GetByChurchRegistrationNumberAsync(number) != null);
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
    }
}