﻿using StThomasMission.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Repository for Attendance-specific operations.
    /// </summary>
    public interface IAttendanceRepository : IRepository<Attendance>
    {
        Task<IEnumerable<Attendance>> GetByStudentIdAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Attendance>> GetByGradeAsync(string grade, DateTime date);
    }
}
