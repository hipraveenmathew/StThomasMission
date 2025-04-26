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
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudentService _studentService;
        private readonly IGroupService _groupService;
        private readonly IAuditService _auditService;

        public AttendanceService(IUnitOfWork unitOfWork, IStudentService studentService, IGroupService groupService, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _studentService = studentService;
            _groupService = groupService;
            _auditService = auditService;
        }

        public async Task AddAttendanceAsync(int studentId, DateTime date, string description, AttendanceStatus status)
        {
            if (date > DateTime.UtcNow)
                throw new ArgumentException("Attendance date cannot be in the future.", nameof(date));
            if (string.IsNullOrEmpty(description))
                throw new ArgumentException("Description is required.", nameof(description));

            await _studentService.GetStudentByIdAsync(studentId);

            var existingAttendance = (await _unitOfWork.Attendances.GetByStudentIdAsync(studentId))
                .FirstOrDefault(a => a.Date.Date == date.Date);
            if (existingAttendance != null)
                throw new InvalidOperationException("Attendance for this student and date already exists.");

            var attendance = new Attendance
            {
                StudentId = studentId,
                Date = date.Date,
                Description = description,
                Status = status
            };

            await _unitOfWork.Attendances.AddAsync(attendance);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(Attendance), attendance.Id.ToString(), $"Added attendance for student {studentId} on {date:yyyy-MM-dd}");
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

            await _auditService.LogActionAsync("System", "Update", nameof(Attendance), attendanceId.ToString(), $"Updated attendance for student {attendance.StudentId}");
        }

        public async Task DeleteAttendanceAsync(int attendanceId)
        {
            var attendance = await GetAttendanceByIdAsync(attendanceId);

            await _unitOfWork.Attendances.DeleteAsync(attendanceId);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Delete", nameof(Attendance), attendanceId.ToString(), $"Deleted attendance for student {attendance.StudentId}");
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByStudentAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            await _studentService.GetStudentByIdAsync(studentId);
            return await _unitOfWork.Attendances.GetByStudentIdAsync(studentId, startDate, endDate);
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByGradeAsync(string grade, DateTime date)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            if (date > DateTime.UtcNow)
                throw new ArgumentException("Attendance date cannot be in the future.", nameof(date));

            return await _unitOfWork.Attendances.GetByGradeAsync(grade, date);
        }

        private async Task<Attendance> GetAttendanceByIdAsync(int attendanceId)
        {
            var attendance = await _unitOfWork.Attendances.GetByIdAsync(attendanceId);
            if (attendance == null)
                throw new ArgumentException("Attendance record not found.", nameof(attendanceId));
            return attendance;
        }

    }
}