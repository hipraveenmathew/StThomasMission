using System;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IExportService
    {
        Task<byte[]> ExportFamiliesToCsvAsync();
        Task<byte[]> ExportStudentsToCsvAsync();
        Task<byte[]> ExportAttendanceToCsvAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
