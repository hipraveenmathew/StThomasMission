using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Families.Models;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Families.Controllers
{
    [Area("Families")]
    [Authorize(Roles = "ParishAdmin,ParishPriest")]
    public class FamilyMembersController : Controller
    {
        private readonly IFamilyService _familyService;
        private readonly IFamilyMemberService _familyMemberService;

        public FamilyMembersController(IFamilyService familyService, IFamilyMemberService familyMemberService)
        {
            _familyService = familyService;
            _familyMemberService = familyMemberService;
        }

        [HttpGet]
        public async Task<IActionResult> Add(int familyId)
        {
            var family = await _familyService.GetFamilyByIdAsync(familyId);
            if (family == null)
            {
                return NotFound("Family not found.");
            }

            var model = new FamilyMemberViewModel { FamilyId = familyId };
            ViewData["FamilyName"] = family.FamilyName;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(FamilyMemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var family = await _familyService.GetFamilyByIdAsync(model.FamilyId);
                ViewData["FamilyName"] = family?.FamilyName;
                return View(model);
            }

            try
            {
                var familyMember = new FamilyMember
                {
                    FamilyId = model.FamilyId,
                    FirstName = model.FirstName?.Trim(),
                    LastName = model.LastName?.Trim(),
                    BaptismalName = model.BaptismalName?.Trim(),
                    Relation = model.Relation,
                    DateOfBirth = model.DateOfBirth,
                    Contact = model.Contact?.Trim(),
                    Email = model.Email?.Trim(),
                    Role = model.Role?.Trim(),
                    DateOfBaptism = model.DateOfBaptism,
                    DateOfChrismation = model.DateOfChrismation,
                    DateOfHolyCommunion = model.DateOfHolyCommunion,
                    DateOfMarriage = model.DateOfMarriage,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                await _familyMemberService.AddFamilyMemberAsync(familyMember);

                return RedirectToAction("Details", "Families", new { id = model.FamilyId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to add family member: {ex.Message}");
                var family = await _familyService.GetFamilyByIdAsync(model.FamilyId);
                ViewData["FamilyName"] = family?.FamilyName;
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var familyMember = await _familyMemberService.GetFamilyMemberByIdAsync(id);
            if (familyMember == null)
            {
                return NotFound("Family member not found.");
            }

            var family = await _familyService.GetFamilyByIdAsync(familyMember.FamilyId);
            ViewData["FamilyName"] = family?.FamilyName;

            var model = new FamilyMemberViewModel
            {
                Id = familyMember.Id,
                FamilyId = familyMember.FamilyId,
                FirstName = familyMember.FirstName,
                LastName = familyMember.LastName,
                BaptismalName = familyMember.BaptismalName,
                Relation = familyMember.Relation,
                DateOfBirth = familyMember.DateOfBirth,
                DateOfDeath = familyMember.DateOfDeath,
                Contact = familyMember.Contact,
                Email = familyMember.Email,
                Role = familyMember.Role,
                DateOfBaptism = familyMember.DateOfBaptism,
                DateOfChrismation = familyMember.DateOfChrismation,
                DateOfHolyCommunion = familyMember.DateOfHolyCommunion,
                DateOfMarriage = familyMember.DateOfMarriage
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FamilyMemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var family = await _familyService.GetFamilyByIdAsync(model.FamilyId);
                ViewData["FamilyName"] = family?.FamilyName;
                return View(model);
            }

            try
            {
                var member = new FamilyMember
                {
                    Id = model.Id,
                    FamilyId = model.FamilyId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BaptismalName = model.BaptismalName,
                    Relation = model.Relation,
                    DateOfBirth = model.DateOfBirth,
                    DateOfDeath = model.DateOfDeath,
                    Contact = model.Contact,
                    Email = model.Email,
                    Role = model.Role,
                    DateOfBaptism = model.DateOfBaptism,
                    DateOfChrismation = model.DateOfChrismation,
                    DateOfHolyCommunion = model.DateOfHolyCommunion,
                    DateOfMarriage = model.DateOfMarriage,
                    UpdatedBy = User.Identity?.Name ?? "System"
                };

                await _familyMemberService.UpdateFamilyMemberAsync(member);

                return RedirectToAction("Details", "Families", new { id = model.FamilyId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to update family member: {ex.Message}");
                var family = await _familyService.GetFamilyByIdAsync(model.FamilyId);
                ViewData["FamilyName"] = family?.FamilyName;
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> TransitionChildToNewFamily(int familyMemberId)
        {
            var familyMember = await _familyMemberService.GetFamilyMemberByIdAsync(familyMemberId); // Assume this method exists
            if (familyMember == null)
            {
                return NotFound("Family member not found.");
            }

            var model = new TransitionChildViewModel
            {
                FamilyMemberId = familyMemberId,
                ChildName = familyMember.FullName,
                NewFamilyName = $"{familyMember.LastName} Family",
                WardId = 1 // Default or fetch from current family
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransitionChildToNewFamily(TransitionChildViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string? churchRegistrationNumber = null;
                string? temporaryId = null;
                if (model.IsRegistered)
                {
                    churchRegistrationNumber = await _familyService.NewChurchIdAsync();
                }
                else
                {
                    temporaryId = $"TMP-{new Random().Next(1000, 9999)}"; // Simple temporary ID generation
                }

                await _familyService.TransitionChildToNewFamilyAsync(
                    model.FamilyMemberId,
                    model.NewFamilyName,
                    model.WardId,
                    model.IsRegistered,
                    churchRegistrationNumber,
                    temporaryId
                );

                TempData["Success"] = "Child successfully transitioned to a new family.";
                return RedirectToAction("Details", new { id = model.FamilyMemberId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to transition child: {ex.Message}");
                return View(model);
            }
        }
    }
}