using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> GenerateStudentReportAsync(int studentId, string format); // PDF or Excel
        Task<byte[]> GenerateClassReportAsync(string grade, int academicYear, string format);
        Task<byte[]> GenerateCatechismReportAsync(int academicYear, string format);
        Task<byte[]> GenerateFamilyReportAsync(string? ward, string? status, string format);
    }
}