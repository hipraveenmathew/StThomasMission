using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for generating reports.
    /// </summary>
    public interface IReportingService
    {
        Task<byte[]> GenerateStudentReportAsync(int studentId, string format);
        Task<byte[]> GenerateClassReportAsync(string grade, int academicYear, string format);
        Task<byte[]> GenerateCatechismReportAsync(int academicYear, string format);
        Task<byte[]> GenerateFamilyReportAsync(string? ward, string? status, string format);
    }
}
