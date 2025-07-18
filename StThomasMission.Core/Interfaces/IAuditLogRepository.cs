using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<IPaginatedList<AuditLogDto>> GetLogsPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? userId = null,
            string? entityName = null,
             string? sortOrder = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}