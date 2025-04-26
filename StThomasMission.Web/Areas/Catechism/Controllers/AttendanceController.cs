using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Catechism.Models;
using StThomasMission.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace StThomasMission.Web.Areas.Catechism.Controllers
{
    [Area("Catechism")]
    [Authorize(Roles = "Teacher,HeadTeacher,ParishPriest")]
    public class AttendanceController : Controller
    {
        private readonly IStudentService _studentService;

        public AttendanceController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        public IActionResult MarkAttendance(string grade, DateTime? date)
        {
            date ??= DateTime.Today;
            var model = new MarkAttendanceViewModel
            {
                Grade = grade,
                Date = date.Value
            };

            if (string.IsNullOrEmpty(grade))
            {
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(MarkAttendanceViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(model.Grade))
            {
                return View(model);
            }

            try
            {
                var students = await _studentService.GetStudentsByGradeAsync(model.Grade);
                if (!students.Any())
                {
                    ModelState.AddModelError("Grade", "No students found for this grade.");
                    return View(model);
                }

                var attendanceRecords = await _studentService.GetAttendanceByDateAsync(model.Date);
                model.AttendanceRecords = students.Select(s => new AttendanceRecord
                {
                    StudentId = s.Id,
                    StudentName = $"{s.FamilyMember.FirstName} {s.FamilyMember.LastName}",
                    IsPresent = attendanceRecords.Any(a => a.StudentId == s.Id && a.Status == Core.Enums.AttendanceStatus.Present)
                }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to load students: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Catechism/Attendance/SubmitAttendance")]
        public async Task<IActionResult> SubmitAttendance(MarkAttendanceViewModel model)
        {
            if (!ModelState.IsValid || model.AttendanceRecords == null || !model.AttendanceRecords.Any())
            {
                ModelState.AddModelError(string.Empty, "No attendance data provided.");
                return RedirectToAction("MarkAttendance", new { grade = model.Grade, date = model.Date });
            }

            try
            {
                foreach (var record in model.AttendanceRecords)
                {
                    await _studentService.AddAttendanceAsync(
                        record.StudentId,
                        model.Date,
                        record.IsPresent ? "Marked Present" : "Marked Absent",
                        record.IsPresent);
                }

                TempData["Success"] = $"Attendance marked for {model.Grade} on {model.Date:yyyy-MM-dd}.";
                return RedirectToAction("MarkAttendance", new { grade = model.Grade, date = model.Date });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to mark attendance: {ex.Message}";
                return RedirectToAction("MarkAttendance", new { grade = model.Grade, date = model.Date });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index(string grade, DateTime? date, int pageNumber = 1, int pageSize = 10)
        {
            ViewData["Grade"] = grade;
            ViewData["Date"] = date?.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(grade) || !date.HasValue)
            {
                return View(new PaginatedList<Attendance>(new List<Attendance>(), 0, pageNumber, pageSize));
            }

            try
            {
                var students = await _studentService.GetStudentsByGradeAsync(grade);
                var studentIds = students.Select(s => s.Id).ToList();

                var attendanceQuery = _studentService.GetAttendanceQueryable(a =>
                    a.Date.Date == date.Value.Date && studentIds.Contains(a.StudentId));

                int totalItems = await attendanceQuery.CountAsync();
                var pagedAttendances = await attendanceQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var model = new PaginatedList<Attendance>(pagedAttendances, totalItems, pageNumber, pageSize);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load attendance records: {ex.Message}";
                return View(new PaginatedList<Attendance>(new List<Attendance>(), 0, pageNumber, pageSize));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Report(string grade, int academicYear, int pageNumber = 1, int pageSize = 10)
        {
            ViewData["Grade"] = grade;
            ViewData["Year"] = academicYear;

            if (string.IsNullOrEmpty(grade) || academicYear <= 0)
            {
                return View(new PaginatedList<Student>(new List<Student>(), 0, pageNumber, pageSize));
            }

            try
            {
                var studentsQuery = _studentService.GetStudentsQueryable(s =>
                    s.Grade == grade && s.AcademicYear == academicYear);

                int totalItems = await studentsQuery.CountAsync();
                var pagedStudents = await studentsQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                foreach (var student in pagedStudents)
                {
                    student.Attendances = (await _studentService.GetAttendanceByStudentAsync(student.Id)).ToList();
                }

                var model = new PaginatedList<Student>(pagedStudents, totalItems, pageNumber, pageSize);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to generate attendance report: {ex.Message}";
                return View(new PaginatedList<Student>(new List<Student>(), 0, pageNumber, pageSize));
            }
        }
    }
}