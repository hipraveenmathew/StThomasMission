using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public AttendanceService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<IEnumerable<ClassAttendanceRecordDto>> GetAttendanceForGradeOnDateAsync(int gradeId, DateTime date)
        {
            return await _unitOfWork.Attendances.GetAttendanceForGradeOnDateAsync(gradeId, date);
        }

        public async Task MarkClassAttendanceAsync(MarkClassAttendanceRequest request, string userId)
        {
            // 1. Validate the request
            if (request.Records == null || !request.Records.Any())
            {
                throw new ArgumentException("At least one attendance record is required.");
            }
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                request.Description = "Catechism Class";
            }

            // 2. Check for existing attendance for any student in the list on this date to prevent duplicates
            var studentIds = request.Records.Select(r => r.StudentId).ToList();
            var existing = await _unitOfWork.Attendances.GetAttendanceForGradeOnDateAsync(request.GradeId, request.Date);
            if (existing.Any())
            {
                throw new InvalidOperationException($"Attendance has already been marked for this class on {request.Date:yyyy-MM-dd}. Please edit the existing records.");
            }

            // 3. Create and add all attendance records
            foreach (var record in request.Records)
            {
                var attendance = new Attendance
                {
                    StudentId = record.StudentId,
                    Date = request.Date.Date,
                    Description = request.Description,
                    Status = record.Status,
                    Remarks = record.Remarks,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.Attendances.AddAsync(attendance);
            }

            // 4. Save all changes in a single transaction
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "BulkCreate", nameof(Attendance), request.GradeId.ToString(), $"Marked attendance for grade ID {request.GradeId} on {request.Date:yyyy-MM-dd}");
        }

        public async Task UpdateAttendanceAsync(int attendanceId, UpdateAttendanceRequest request, string userId)
        {
            var attendance = await _unitOfWork.Attendances.GetByIdAsync(attendanceId);
            if (attendance == null)
            {
                throw new NotFoundException(nameof(Attendance), attendanceId);
            }

            attendance.Status = request.Status;
            attendance.Description = request.Description;
            attendance.Remarks = request.Remarks;
            // Note: We don't update CreatedBy or CreatedAt. Those are fixed.

            await _unitOfWork.Attendances.UpdateAsync(attendance);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Update", nameof(Attendance), attendanceId.ToString(), $"Updated attendance for student ID {attendance.StudentId}");
        }

        public async Task DeleteAttendanceAsync(int attendanceId, string userId)
        {
            var attendance = await _unitOfWork.Attendances.GetByIdAsync(attendanceId);
            if (attendance == null)
            {
                throw new NotFoundException(nameof(Attendance), attendanceId);
            }

            // This is now a hard delete as per the repository change
            await _unitOfWork.Attendances.DeleteAsync(attendanceId);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Delete", nameof(Attendance), attendanceId.ToString(), $"Deleted attendance record for student ID {attendance.StudentId}");
        }
    }
}