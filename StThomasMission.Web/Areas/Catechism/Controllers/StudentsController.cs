using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Catechism.Models;

namespace StThomasMission.Web.Areas.Catechism.Controllers
{
    [Area("Catechism")]
    [Authorize(Roles = "Teacher, HeadTeacher")]
    public class StudentsController : Controller
    {
        private readonly ICatechismService _catechismService;

        public StudentsController(ICatechismService catechismService)
        {
            _catechismService = catechismService;
        }

        public async Task<IActionResult> Index(string grade = "Year 1")
        {
            var students = await _catechismService.GetStudentsByGradeAsync(grade);
            var model = students.Select(s => new StudentViewModel
            {
                Id = s.Id,
                FirstName = s.FamilyMember.FirstName,
                LastName = s.FamilyMember.LastName,
                Grade = s.Grade,
                Group = s.Group,
                StudentOrganisation = s.StudentOrganisation,
                Status = s.Status
            }).ToList();
            ViewBag.Grade = grade;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MarkAttendance(string grade)
        {
            var students = await _catechismService.GetStudentsByGradeAsync(grade);
            if (!students.Any()) return NotFound("No students found for this grade.");

            var model = new ClassAttendanceViewModel
            {
                Grade = grade,
                Date = DateTime.Today,
                Description = "Catechism Class",
                Students = students.Select(s => new StudentAttendanceViewModel
                {
                    StudentId = s.Id,
                    Name = $"{s.FamilyMember.FirstName} {s.FamilyMember.LastName}",
                    IsPresent = false
                }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(ClassAttendanceViewModel model)
        {
            if (ModelState.IsValid)
            {
                foreach (var student in model.Students)
                {
                    await _catechismService.MarkAttendanceAsync(
                        student.StudentId,
                        model.Date,
                        model.Description,
                        student.IsPresent);
                }
                return RedirectToAction(nameof(Index), new { grade = model.Grade });
            }
            return View(model);
        }

        public async Task<IActionResult> AddAssessment(int id)
        {
            var student = await _catechismService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            ViewBag.StudentName = $"{student.FamilyMember.FirstName} {student.FamilyMember.LastName}";
            return View(new AssessmentViewModel { StudentId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAssessment(AssessmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _catechismService.AddAssessmentAsync(model.StudentId, model.Name, model.Marks, model.TotalMarks, model.IsMajor);
                return RedirectToAction(nameof(Index), new { grade = ViewBag.Grade });
            }
            var student = await _catechismService.GetStudentByIdAsync(model.StudentId);
            ViewBag.StudentName = $"{student?.FamilyMember.FirstName} {student?.FamilyMember.LastName}";
            return View(model);
        }

        public async Task<IActionResult> MarkPassFail(int id)
        {
            var student = await _catechismService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            ViewBag.StudentName = $"{student.FamilyMember.FirstName} {student.FamilyMember.LastName}";
            return View(new PassFailViewModel { StudentId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPassFail(PassFailViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _catechismService.MarkPassFailAsync(model.StudentId, model.Passed);
                return RedirectToAction(nameof(Index), new { grade = ViewBag.Grade });
            }
            var student = await _catechismService.GetStudentByIdAsync(model.StudentId);
            ViewBag.StudentName = $"{student?.FamilyMember.FirstName} {student?.FamilyMember.LastName}";
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var student = await _catechismService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var attendances = await _catechismService.GetAttendanceByStudentAsync(id);
            var assessments = await _catechismService.GetAssessmentsByStudentAsync(id);

            var model = new StudentDetailsViewModel
            {
                Id = student.Id,
                FirstName = student.FamilyMember.FirstName,
                LastName = student.FamilyMember.LastName,
                Grade = student.Grade,
                Group = student.Group,
                StudentOrganisation = student.StudentOrganisation,
                Status = student.Status,
                Attendances = attendances.ToList(),
                Assessments = assessments.ToList()
            };

            return View(model);
        }
        // Add to the existing StudentsController
        public IActionResult AddGroupActivity(string group)
        {
            ViewBag.Group = group;
            return View(new GroupActivityViewModel { Group = group });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGroupActivity(GroupActivityViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _catechismService.AddGroupActivityAsync(model.Group, model.Name, model.Points);
                return RedirectToAction(nameof(Index), new { grade = ViewBag.Grade });
            }
            ViewBag.Group = model.Group;
            return View(model);
        }
    }
}