using StThomasMission.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAttendanceService
    {
        Task<IEnumerable<ClassAttendanceRecordDto>> GetAttendanceForGradeOnDateAsync(int gradeId, DateTime date);

        Task MarkClassAttendanceAsync(MarkClassAttendanceRequest request, string userId);

        Task UpdateAttendanceAsync(int attendanceId, UpdateAttendanceRequest request, string userId);

        Task DeleteAttendanceAsync(int attendanceId, string userId);
    }
}