using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Families.Controllers
{
    [Area("Families")]
    [Authorize(Roles = "ParishAdmin,ParishPriest")]
    public class FamilyMembersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public FamilyMembersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Add(int familyId)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null)
            {
                return NotFound();
            }
            var model = new FamilyMember { FamilyId = familyId };
            ViewBag.FamilyName = family.FamilyName;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int familyId, string firstName, string lastName, string contact, string email, string role)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null)
            {
                return NotFound();
            }

            var familyMember = new FamilyMember
            {
                FamilyId = familyId,
                FirstName = firstName,
                LastName = lastName,
                Contact = contact,
                Email = email,
                Role = role
            };

            await _unitOfWork.FamilyMembers.AddAsync(familyMember);
            await _unitOfWork.CompleteAsync();
            TempData["Success"] = "Family member added successfully!";
            return RedirectToAction("Details", "Families", new { id = familyId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(id);
            if (familyMember == null)
            {
                return NotFound();
            }
            var family = await _unitOfWork.Families.GetByIdAsync(familyMember.FamilyId);
            ViewBag.FamilyName = family.FamilyName;
            return View(familyMember);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string firstName, string lastName, string contact, string email, string role)
        {
            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(id);
            if (familyMember == null)
            {
                return NotFound();
            }

            familyMember.FirstName = firstName;
            familyMember.LastName = lastName;
            familyMember.Contact = contact;
            familyMember.Email = email;
            familyMember.Role = role;

            await _unitOfWork.FamilyMembers.UpdateAsync(familyMember);
            await _unitOfWork.CompleteAsync();
            TempData["Success"] = "Family member updated successfully!";
            return RedirectToAction("Details", "Families", new { id = familyMember.FamilyId });
        }
    }
}