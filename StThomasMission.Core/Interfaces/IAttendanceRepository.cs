using StThomasMission.Core.Entities;

namespace StThomasMission.Core.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<Attendance> AddAsync(Attendance attendance);
        Task<IEnumerable<Attendance>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<Attendance>> GetByDateAsync(DateTime date);
    }
}