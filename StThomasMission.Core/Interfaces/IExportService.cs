using StThomasMission.Core.Enums;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IExportService
    {
        Task<byte[]> ExportFamiliesAsync(ReportFormat format);
        Task<byte[]> ExportStudentsAsync(ReportFormat format);
        Task<byte[]> ExportAttendanceAsync(DateTime? startDate, DateTime? endDate, ReportFormat format);
    }
}