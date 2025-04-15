using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
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
            {
                return View(new MarkAttendanceViewModel());
            }

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
                    StudentName = $"{s.FirstName} {s.LastName}",
                    IsPresent = attendanceRecords.Any(a => a.StudentId == s.Id && a.IsPresent)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAttendance(string grade, DateTime date, int[] studentIds, bool[] isPresent)
        {
            if (studentIds.Length != isPresent.Length)
            {
                TempData["Error"] = "Invalid attendance data.";
                return RedirectToAction("MarkAttendance", new { grade, date });
            }

            for (int i = 0; i < studentIds.Length; i++)
            {
                var attendance = new Attendance
                {
                    StudentId = studentIds[i],
                    Date = date,
                    IsPresent = isPresent[i]
                };
                await _unitOfWork.Attendances.AddAsync(attendance);
            }

            await _unitOfWork.CompleteAsync();
            TempData["Success"] = $"Attendance marked for {grade} on {date.ToShortDateString()}.";
            return RedirectToAction("MarkAttendance", new { grade, date });
        }
    }

    public class MarkAttendanceViewModel
    {
        public string Grade { get; set; }
        public DateTime Date { get; set; }
        public List<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }

    public class AttendanceRecord
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public bool IsPresent { get; set; }
    }
}