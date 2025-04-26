using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Parents.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Parents.Controllers
{
    [Area("Parents")]
    [Authorize(Roles = "Parent")]
    public class PortalController : Controller
    {
        private readonly IFamilyService _familyService;
        private readonly IUnitOfWork _unitOfWork;

        public PortalController(IFamilyService familyService, IUnitOfWork unitOfWork)
        {
            _familyService = familyService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var familyMember = await _unitOfWork.FamilyMembers.GetByUserIdAsync(userId);
            if (familyMember == null) return NotFound("Family member not found.");

            var family = await _familyService.GetFamilyByIdAsync(familyMember.FamilyId); // Fixed method name
            if (family == null) return NotFound("Family not found.");

            var students = await _unitOfWork.Students.GetByFamilyIdAsync(family.Id);

            var model = new ParentPortalViewModel
            {
                Family = family,
                Students = students.ToList()
            };

            return View(model);
        }
    }
}