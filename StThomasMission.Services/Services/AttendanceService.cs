using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    /// <summary>
    /// Service for managing student attendance.
    /// </summary>
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatechismService _catechismService;

        public AttendanceService(IUnitOfWork unitOfWork, ICatechismService catechismService)
        {
            _unitOfWork = unitOfWork;
            _catechismService = catechismService;
        }

        public async Task MarkAttendanceAsync(int studentId, DateTime date, AttendanceStatus status, string description)
        {
            if (date > DateTime.UtcNow)
                throw new ArgumentException("Attendance date cannot be in the future.", nameof(date));
            if (string.IsNullOrEmpty(description))
                throw new ArgumentException("Description is required.", nameof(description));

            await _catechismService.GetStudentByIdAsync(studentId); // Validates student exists

            var existingAttendance = (await _unitOfWork.Attendances.GetByStudentIdAsync(studentId))
                .FirstOrDefault(a => a.Date.Date == date.Date);
            if (existingAttendance != null)
                throw new InvalidOperationException("Attendance for this student and date already exists.");

            var attendance = new Attendance
            {
                StudentId = studentId,
                Date = date.Date,
                Status = status,
                Description = description
            };

            await _unitOfWork.Attendances.AddAsync(attendance);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAttendanceAsync(int attendanceId, AttendanceStatus status, string description)
        {
            var attendance = await GetAttendanceByIdAsync(attendanceId);

            if (string.IsNullOrEmpty(description))
                throw new ArgumentException("Description is required.", nameof(description));

            attendance.Status = status;
            attendance.Description = description;

            await _unitOfWork.Attendances.UpdateAsync(attendance);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAttendanceAsync(int attendanceId)
        {
            var attendance = await GetAttendanceByIdAsync(attendanceId);
            await _unitOfWork.Attendances.DeleteAsync(attendance);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<Attendance> GetAttendanceByIdAsync(int attendanceId)
        {
            var attendance = await _unitOfWork.Attendances.GetByIdAsync(attendanceId);
            if (attendance == null)
                throw new ArgumentException("Attendance record not found.", nameof(attendanceId));
            return attendance;
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByStudentIdAsync(int studentId)
        {
            await _catechismService.GetStudentByIdAsync(studentId); // Validates student exists
            return await _unitOfWork.Attendances.GetByStudentIdAsync(studentId);
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByGradeAsync(string grade, DateTime date)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            if (date > DateTime.UtcNow)
                throw new ArgumentException("Attendance date cannot be in the future.", nameof(date));

            var students = await _catechismService.GetStudentsByGradeAsync(grade);
            var studentIds = students.Select(s => s.Id);
            var attendances = await _unitOfWork.Attendances.GetAllAsync();
            return attendances.Where(a => studentIds.Contains(a.StudentId) && a.Date.Date == date.Date);
        }

        public async Task MarkGroupAttendanceAsync(string group, DateTime date, string description)
        {
            if (string.IsNullOrEmpty(group))
                throw new ArgumentException("Group is required.", nameof(group));
            if (date > DateTime.UtcNow)
                throw new ArgumentException("Attendance date cannot be in the future.", nameof(date));
            if (string.IsNullOrEmpty(description))
                throw new ArgumentException("Description is required.", nameof(description));

            var students = await _catechismService.GetStudentsByGroupAsync(group);
            foreach (var student in students)
            {
                var existingAttendance = (await _unitOfWork.Attendances.GetByStudentIdAsync(student.Id))
                    .FirstOrDefault(a => a.Date.Date == date.Date);
                if (existingAttendance == null)
                {
                    var attendance = new Attendance
                    {
                        StudentId = student.Id,
                        Date = date.Date,
                        Status = AttendanceStatus.Present,
                        Description = description
                    };
                    await _unitOfWork.Attendances.AddAsync(attendance);
                }
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}