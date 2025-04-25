using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing Attendance operations.
    /// </summary>
    public interface IAttendanceService
    {
        Task AddAttendanceAsync(int studentId, DateTime date, string description, AttendanceStatus status);
        Task UpdateAttendanceAsync(int attendanceId, AttendanceStatus status, string description);
        Task DeleteAttendanceAsync(int attendanceId);
        Task<IEnumerable<Attendance>> GetAttendanceByStudentAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Attendance>> GetAttendanceByGradeAsync(string grade, DateTime date);
    }
}