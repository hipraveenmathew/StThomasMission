using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Catechism.Models;
using StThomasMission.Web.Models;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Enums;
using StThomasMission.Services;

namespace StThomasMission.Web.Areas.Catechism.Controllers
{
    [Area("Catechism")]
    [Authorize(Roles = "Teacher,HeadTeacher")]
    public class StudentsController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ICatechismService _catechismService;
        private readonly IAssessmentService _assessmentService;
        private readonly IGroupActivityService _groupActivityService;

        public StudentsController(IStudentService studentService, ICatechismService catechismService, IAssessmentService assessmentService, IGroupActivityService groupActivityService)
        {
            _studentService = studentService;
            _catechismService = catechismService;
            _assessmentService = assessmentService;
            _groupActivityService = groupActivityService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["GradeSortParm"] = sortOrder == "grade" ? "grade_desc" : "grade";

            try
            {
                var studentsQuery = _studentService.GetStudentsQueryable(s => true);

                // Filter students by year for teachers
                if (User.IsInRole("Teacher"))
                {
                    // Extract the teacher's year from their username (e.g., teacher1 -> Year 1)
                    var username = User.Identity.Name; // e.g., teacher1@stthomasmission.com
                    var teacherNumber = username.Replace("@stthomasmission.com", "").Replace("teacher", ""); // e.g., "1"
                    if (int.TryParse(teacherNumber, out int year))
                    {
                        var teacherYear = $"Year {year}"; // e.g., "Year 1"
                        studentsQuery = studentsQuery.Where(s => s.Grade == teacherYear);
                    }
                    else
                    {
                        // Fallback: If the year cannot be parsed, show no students
                        TempData["Error"] = "Unable to determine your assigned year. Please contact the administrator.";
                        return View(new PaginatedList<Student>(new List<Student>(), 0, pageNumber, pageSize));
                    }
                }
                // HeadTeacher, ParishPriest, and Admin see all students
                // No additional filtering needed

                // Apply search filter
                if (!string.IsNullOrEmpty(searchString))
                {
                    var searchLower = searchString.ToLower();
                    studentsQuery = studentsQuery.Where(s =>
                        (s.FamilyMember.FirstName + " " + s.FamilyMember.LastName).ToLower().Contains(searchLower) ||
                        s.Grade.ToLower().Contains(searchLower));
                }

                // Apply sorting
                studentsQuery = sortOrder switch
                {
                    "name_desc" => studentsQuery.OrderByDescending(s => s.FamilyMember.FirstName),
                    "grade" => studentsQuery.OrderBy(s => s.Grade),
                    "grade_desc" => studentsQuery.OrderByDescending(s => s.Grade),
                    _ => studentsQuery.OrderBy(s => s.FamilyMember.FirstName),
                };

                // Apply pagination
                int totalItems = await studentsQuery.CountAsync();
                var pagedStudents = await studentsQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var model = new PaginatedList<Student>(pagedStudents, totalItems, pageNumber, pageSize);
                ViewData["SearchString"] = searchString;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load students: {ex.Message}";
                return View(new PaginatedList<Student>(new List<Student>(), 0, pageNumber, pageSize));
            }
        }
        [HttpGet]
        public async Task<IActionResult> MarkAttendance(string grade)
        {
            try
            {
                var students = await _studentService.GetStudentsByGradeAsync(grade);
                if (!students.Any())
                {
                    TempData["Error"] = "No students found for this grade.";
                    return RedirectToAction("Index");
                }

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
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load students for attendance: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(ClassAttendanceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                foreach (var student in model.Students)
                {
                    await _studentService.AddAttendanceAsync(
                        student.StudentId,
                        model.Date,
                        model.Description,
                        student.IsPresent);
                }
                TempData["Success"] = "Attendance marked successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to mark attendance: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddAssessment(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    return NotFound("Student not found.");
                }

                ViewData["StudentName"] = $"{student.FamilyMember.FirstName} {student.FamilyMember.LastName}";
                return View(new AssessmentViewModel { StudentId = id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load student: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAssessment(AssessmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var student = await _studentService.GetStudentByIdAsync(model.StudentId);
                ViewData["StudentName"] = student != null ? $"{student.FamilyMember.FirstName} {student.FamilyMember.LastName}" : "Unknown";
                return View(model);
            }

            try
            {
                await _assessmentService.AddAssessmentAsync(
                    model.StudentId,
                    model.Name,
                    model.Marks,
                    model.TotalMarks,
                    DateTime.Today,
                    model.IsMajor ? AssessmentType.SemesterExam : AssessmentType.ClassAssessment); // Updated to use AssessmentType
                TempData["Success"] = "Assessment added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to add assessment: {ex.Message}");
                var student = await _studentService.GetStudentByIdAsync(model.StudentId);
                ViewData["StudentName"] = student != null ? $"{student.FamilyMember.FirstName} {student.FamilyMember.LastName}" : "Unknown";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> MarkPassFail(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    return NotFound("Student not found.");
                }

                ViewData["StudentName"] = $"{student.FamilyMember.FirstName} {student.FamilyMember.LastName}";
                return View(new PassFailViewModel { StudentId = id, AcademicYear = student.AcademicYear });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load student: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPassFail(PassFailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var student = await _studentService.GetStudentByIdAsync(model.StudentId);
                ViewData["StudentName"] = student != null ? $"{student.FamilyMember.FirstName} {student.FamilyMember.LastName}" : "Unknown";
                return View(model);
            }

            try
            {
                await _studentService.MarkPassFailAsync(model.StudentId, model.AcademicYear, model.PassThreshold, model.Remarks);
                TempData["Success"] = "Pass/Fail status recorded successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to record pass/fail status: {ex.Message}");
                var student = await _studentService.GetStudentByIdAsync(model.StudentId);
                ViewData["StudentName"] = student != null ? $"{student.FamilyMember.FirstName} {student.FamilyMember.LastName}" : "Unknown";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    return NotFound("Student not found.");
                }

                var attendances = await _studentService.GetAttendanceByStudentAsync(id);
                var assessments = await _studentService.GetAssessmentsByStudentAsync(id);

                var model = new StudentDetailsViewModel
                {
                    Id = student.Id,
                    FirstName = student.FamilyMember?.FirstName ?? "N/A",
                    LastName = student.FamilyMember?.LastName ?? "N/A",
                    Grade = student.Grade,
                    GroupId = student.GroupId,
                    StudentOrganisation = student.StudentOrganisation,
                    Status = student.Status.ToString(),
                    Attendances = attendances?.ToList() ?? new List<Attendance>(),
                    Assessments = assessments?.ToList() ?? new List<Assessment>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load student details: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult AddGroupActivity(string grade)
        {
            ViewData["Grade"] = grade;
            return View(new GroupActivityViewModel { Grade = grade });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGroupActivity(GroupActivityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Grade"] = model.Grade;
                return View(model);
            }

            try
            {
                await _groupActivityService.AddGroupActivityAsync(
                    model.Name,
                    "Auto-added activity",
                    DateTime.Today,
                    model.GroupId,
                    model.Points);
                TempData["Success"] = "Group activity added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to add group activity: {ex.Message}");
                ViewData["Grade"] = model.Grade;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    return NotFound("Student not found.");
                }

                var model = new EditStudentViewModel
                {
                    Id = student.Id,
                    FamilyMemberId = student.FamilyMemberId,
                    FirstName = student.FamilyMember?.FirstName ?? "N/A",
                    LastName = student.FamilyMember?.LastName ?? "N/A",
                    Grade = student.Grade,
                    AcademicYear = student.AcademicYear,
                    GroupId = student.GroupId,
                    StudentOrganisation = student.StudentOrganisation,
                    Status = student.Status.ToString()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load student: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _studentService.UpdateStudentAsync(
                    model.Id,
                    model.Grade,
                    model.GroupId,
                    model.StudentOrganisation,
                    Enum.Parse<Core.Enums.StudentStatus>(model.Status),
                    null); // Assuming no migration in this context
                TempData["Success"] = "Student updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to update student: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    return NotFound("Student not found.");
                }

                var model = new StudentDetailsViewModel
                {
                    Id = student.Id,
                    FirstName = student.FamilyMember?.FirstName ?? "N/A",
                    LastName = student.FamilyMember?.LastName ?? "N/A",
                    Grade = student.Grade,
                    GroupId = student.GroupId,
                    StudentOrganisation = student.StudentOrganisation,
                    Status = student.Status.ToString()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load student: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _studentService.DeleteStudentAsync(id);
                TempData["Success"] = "Student deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to delete student: {ex.Message}";
            }
            return RedirectToAction("Index");
        }
    }
}