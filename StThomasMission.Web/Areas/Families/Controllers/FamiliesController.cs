using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StThomasMission.Core.Constants;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using StThomasMission.Web.Areas.Church.Models;
using StThomasMission.Web.Areas.Families.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Church.Controllers
{
    [Area("Church")]
    [Authorize(Roles = $"{UserRoles.ParishAdmin},{UserRoles.ParishPriest}")]
    public class FamiliesController : Controller
    {
        private readonly IFamilyService _familyService;
        private readonly IWardService _wardService;
        private readonly ILogger<FamiliesController> _logger;

        public FamiliesController(IFamilyService familyService, IWardService wardService, ILogger<FamiliesController> logger)
        {
            _familyService = familyService;
            _wardService = wardService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(FamilyFilterViewModel filter, string? sortOrder, int pageNumber = 1, int pageSize = 15)
        {
            var pagedFamilies = await _familyService.SearchFamiliesPaginatedAsync(pageNumber, pageSize, filter.SearchTerm, filter.WardId, filter.IsRegistered);

            var model = new FamilyIndexViewModel
            {
                PagedFamilies = pagedFamilies,
                Filter = filter,
                CurrentSort = sortOrder,
                AvailableWards = await GetWardsSelectListAsync()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var familyDto = await _familyService.GetFamilyDetailByIdAsync(id);
                var model = new FamilyDetailsViewModel
                {
                    Family = familyDto
                };
                return View(model);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var model = new FamilyFormViewModel
            {
                AvailableWards = await GetWardsSelectListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(FamilyFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableWards = await GetWardsSelectListAsync();
                return View(model);
            }
            try
            {
                var request = new RegisterFamilyRequest
                {
                    FamilyName = model.FamilyName,
                    WardId = model.WardId,
                    HouseNumber = model.HouseNumber,
                    StreetName = model.StreetName,
                    City = model.City,
                    PostCode = model.PostCode,
                    Email = model.Email,
                    GiftAid = model.GiftAid
                };
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var newFamily = await _familyService.RegisterFamilyAsync(request, userId);

                TempData["Success"] = "Family registered successfully. You can now add family members.";
                return RedirectToAction(nameof(Details), new { id = newFamily.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering new family.");
                ModelState.AddModelError("", "An unexpected error occurred while registering the family.");
                model.AvailableWards = await GetWardsSelectListAsync();
                return View(model);
            }
        }

        // Add these actions to your FamiliesController

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var dto = await _familyService.GetFamilyDetailByIdAsync(id);
                var model = new FamilyFormViewModel
                {
                    Id = dto.Id,
                    FamilyName = dto.FamilyName,
                    WardId = dto.WardId, // Assuming WardId is on the detail DTO
                    HouseNumber = dto.HouseNumber,
                    StreetName = dto.StreetName,
                    City = dto.City,
                    PostCode = dto.PostCode,
                    Email = dto.Email,
                    GiftAid = dto.GiftAid,
                    AvailableWards = await GetWardsSelectListAsync(dto.WardId)
                };
                return View(model);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FamilyFormViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                model.AvailableWards = await GetWardsSelectListAsync(model.WardId);
                return View(model);
            }

            try
            {
                var request = new UpdateFamilyDetailsRequest
                {
                    FamilyName = model.FamilyName,
                    WardId = model.WardId,
                    HouseNumber = model.HouseNumber,
                    StreetName = model.StreetName,
                    City = model.City,
                    PostCode = model.PostCode,
                    Email = model.Email,
                    GiftAid = model.GiftAid
                };
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _familyService.UpdateFamilyDetailsAsync(id, request, userId);

                TempData["Success"] = "Family details updated successfully.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating family {FamilyId}", id);
                ModelState.AddModelError("", "An unexpected error occurred while updating the family.");
                model.AvailableWards = await GetWardsSelectListAsync(model.WardId);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var family = await _familyService.GetFamilyDetailByIdAsync(id);
                return View(family);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _familyService.DeleteFamilyAsync(id, userId);
                TempData["Success"] = "Family has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting family {FamilyId}", id);
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Migrate(int id)
        {
            try
            {
                var family = await _familyService.GetFamilyDetailByIdAsync(id);
                var model = new MigrateFamilyViewModel { FamilyId = family.Id, FamilyName = family.FamilyName };
                return View(model);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Migrate(MigrateFamilyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var request = new MigrateFamilyRequest { MigratedTo = model.MigratedTo };
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _familyService.MigrateFamilyAsync(model.FamilyId, request, userId);

                TempData["Success"] = "Family has been marked as migrated.";
                return RedirectToAction(nameof(Details), new { id = model.FamilyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating family {FamilyId}", model.FamilyId);
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id = model.FamilyId });
            }
        }

        // Update the GetWardsSelectListAsync helper to handle a selected value
        private async Task<SelectList> GetWardsSelectListAsync(int? selectedWardId = null)
        {
            var wards = await _wardService.GetAllWardsAsync();
            return new SelectList(wards, "Id", "Name", selectedWardId);
        }
        private async Task<SelectList> GetWardsSelectListAsync()
        {
            var wards = await _wardService.GetAllWardsAsync();
            return new SelectList(wards, "Id", "Name");
        }
    }
}