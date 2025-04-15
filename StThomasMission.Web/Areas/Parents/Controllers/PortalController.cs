using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using System.Linq;
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
            // Assume the parent's user ID is linked to a FamilyMember
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var familyMember = await _unitOfWork.FamilyMembers.GetByUserIdAsync(userId);
            if (familyMember == null)
            {
                return NotFound("Family member not found.");
            }

            var family = await _familyService.GetByIdAsync(familyMember.FamilyId);
            if (family == null)
            {
                return NotFound("Family not found.");
            }

            var students = await _unitOfWork.Students.GetByFamilyIdAsync(family.Id);
            ViewBag.Students = students;
            return View(family);
        }
    }
}