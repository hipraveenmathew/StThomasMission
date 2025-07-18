using StThomasMission.Core.DTOs;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IMigrationLogService
    {
        Task LogFamilyMigrationAsync(LogMigrationRequest request, string userId);

        Task<IPaginatedList<MigrationLogDto>> GetMigrationLogsAsync(
            int pageNumber,
            int pageSize,
            int? familyId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}