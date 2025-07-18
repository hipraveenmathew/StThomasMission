using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IMigrationLogRepository : IRepository<MigrationLog>
    {
        Task<IPaginatedList<MigrationLogDto>> SearchMigrationLogsPaginatedAsync(
            int pageNumber,
            int pageSize,
            int? familyId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}