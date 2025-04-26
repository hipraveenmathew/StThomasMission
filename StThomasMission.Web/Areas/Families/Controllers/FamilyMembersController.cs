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
                await _familyMemberService.AddFamilyMemberAsync(
                    model.FamilyId,
                    model.FirstName,
                    model.LastName,
                    model.Relation,
                    model.DateOfBirth,
                    model.Contact,
                    model.Email,
                    model.Role);
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
                Relation = familyMember.Relation,
                DateOfBirth = familyMember.DateOfBirth,
                Contact = familyMember.Contact,
                Email = familyMember.Email,
                Role = familyMember.Role
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
                await _familyMemberService.UpdateFamilyMemberAsync(
                    model.Id,
                    model.FirstName,
                    model.LastName,
                    model.Relation,
                    model.DateOfBirth,
                    model.Contact,
                    model.Email,
                    model.Role);
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
    }
}