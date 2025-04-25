using System;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for handling data export operations.
    /// </summary>
    public interface IExportService
    {
        Task<byte[]> ExportFamiliesToExcelAsync();
        Task<byte[]> ExportStudentsToExcelAsync();
        Task<byte[]> ExportAttendanceToExcelAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<byte[]> ExportFamiliesToPdfAsync();
        Task<byte[]> ExportStudentsToPdfAsync();
        Task<byte[]> ExportAttendanceToPdfAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}