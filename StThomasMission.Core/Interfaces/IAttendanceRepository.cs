using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAttendanceRepository : IRepository<Attendance>
    {
        Task<IEnumerable<AttendanceDto>> GetAttendanceByStudentIdAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null);

        Task<IEnumerable<ClassAttendanceRecordDto>> GetAttendanceForGradeOnDateAsync(int gradeId, DateTime date);
    }
}