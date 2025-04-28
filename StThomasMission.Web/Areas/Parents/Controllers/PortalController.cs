using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
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

            var family = await _familyService.GetFamilyByIdAsync(familyMember.FamilyId);
            if (family == null) return NotFound("Family not found.");

            var students = await _unitOfWork.Students.GetByFamilyIdAsync(family.Id);

            // Fetch attendance and assessments for each student
            var studentAttendances = new Dictionary<int, List<Attendance>>();
            var studentAssessments = new Dictionary<int, List<Assessment>>();
            foreach (var student in students)
            {
                var attendances = await _unitOfWork.Attendances.GetByStudentIdAsync(student.Id);
                studentAttendances[student.Id] = attendances.ToList();

                var assessments = await _unitOfWork.Assessments.GetAssessmentsByStudentIdAsync(student.Id);
                studentAssessments[student.Id] = assessments.ToList();
            }

            var model = new ParentPortalViewModel
            {
                Family = family,
                Students = students.ToList(),
                StudentAttendances = studentAttendances,
                StudentAssessments = studentAssessments
            };

            return View(model);
        }
    }
}