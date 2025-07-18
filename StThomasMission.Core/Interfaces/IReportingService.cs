using StThomasMission.Core.Enums;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IReportingService
    {
        Task<byte[]> GenerateStudentReportAsync(int studentId, ReportFormat format);
        Task<byte[]> GenerateClassReportAsync(int gradeId, int academicYear, ReportFormat format);
    }
}