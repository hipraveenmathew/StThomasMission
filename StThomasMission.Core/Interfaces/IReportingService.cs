using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for generating reports.
    /// </summary>
    public interface IReportingService
    {
        Task<byte[]> GenerateStudentReportAsync(int studentId, ReportFormat format);
        Task<byte[]> GenerateClassReportAsync(string grade, int academicYear, ReportFormat format);
        Task<byte[]> GenerateCatechismReportAsync(int academicYear, ReportFormat format);
        Task<byte[]> GenerateFamilyReportAsync(int? wardId, FamilyStatus? status, ReportFormat format);
        Task<byte[]> GenerateWardReportAsync(int wardId, ReportFormat format);
        Task<DashboardSummaryDto> GenerateDashboardSummaryAsync();
    }
}