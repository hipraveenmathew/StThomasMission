using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Catechism.Controllers
{
    [Area("Catechism")]
    [Authorize(Roles = "Teacher,HeadTeacher,ParishPriest")]
    public class AttendanceController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttendanceController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> MarkAttendance(string grade, DateTime? date)
        {
            if (string.IsNullOrEmpty(grade))
                return View(new MarkAttendanceViewModel());

            date ??= DateTime.Today;

            var students = await _unitOfWork.Students.GetByGradeAsync(grade);
            var attendanceRecords = await _unitOfWork.Attendances.GetByDateAsync(date.Value);

            var model = new MarkAttendanceViewModel
            {
                Grade = grade,
                Date = date.Value,
                AttendanceRecords = students.Select(s => new AttendanceRecord
                {
                    StudentId = s.Id,
                    StudentName = $"{s.FamilyMember.FirstName} {s.FamilyMember.LastName}",
                    IsPresent = attendanceRecords.Any(a => a.StudentId == s.Id && a.Status == AttendanceStatus.Present)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(string grade, DateTime date, int[] studentIds, bool[] isPresent)
        {
            if (studentIds.Length != isPresent.Length)
            {
                TempData["Error"] = "Invalid attendance data.";
                return RedirectToAction("MarkAttendance", new { grade, date });
            }

            for (int i = 0; i < studentIds.Length; i++)
            {
                var existing = (await _unitOfWork.Attendances.GetByStudentIdAsync(studentIds[i]))
                    .FirstOrDefault(a => a.Date == date);

                if (existing == null)
                {
                    var attendance = new Attendance
                    {
                        StudentId = studentIds[i],
                        Date = date,
                        Status = isPresent[i] ? AttendanceStatus.Present : AttendanceStatus.Absent,
                        Description = isPresent[i] ? "Marked Present" : "Marked Absent"
                    };
                    await _unitOfWork.Attendances.AddAsync(attendance);
                }
            }

            await _unitOfWork.CompleteAsync();
            TempData["Success"] = $"Attendance marked for {grade} on {date:yyyy-MM-dd}.";
            return RedirectToAction("MarkAttendance", new { grade, date });
        }
        [HttpGet]
        public async Task<IActionResult> Index(string grade, DateTime? date)
        {
            ViewBag.Grade = grade;
            ViewBag.Date = date?.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(grade) || !date.HasValue)
                return View(new List<Attendance>());

            var students = await _unitOfWork.Students.GetByGradeAsync(grade);
            var studentIds = students.Select(s => s.Id).ToList();

            var attendanceRecords = (await _unitOfWork.Attendances.GetAllAsync())
                .Where(a => a.Date.Date == date.Value.Date && studentIds.Contains(a.StudentId))
                .ToList();

            return View(attendanceRecords);
        }

        // ✅ Attendance Summary Report
        [HttpGet]
        public async Task<IActionResult> Report(string grade, int academicYear)
        {
            ViewBag.Grade = grade;
            ViewBag.Year = academicYear;

            if (string.IsNullOrEmpty(grade))
                return View(new List<Student>());

            var students = (await _unitOfWork.Students.GetByGradeAsync(grade))
                .Where(s => s.AcademicYear == academicYear)
                .ToList();

            foreach (var student in students)
            {
                student.Attendances = (await _unitOfWork.Attendances.GetByStudentIdAsync(student.Id)).ToList();
            }

            return View(students);
        }
    }

    public class MarkAttendanceViewModel
    {
        public string Grade { get; set; }
        public DateTime Date { get; set; }
        public List<AttendanceRecord> AttendanceRecords { get; set; } = new();
    }

    public class AttendanceRecord
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public bool IsPresent { get; set; }
    }
}
