using StThomasMission.Core.DTOs;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(string userId, string action, string entityName, string entityId, string details);

        Task<IPaginatedList<AuditLogDto>> GetLogsAsync(
            int pageNumber,
            int pageSize,
            string? userId = null,
            string? entityName = null,
            string? sortOrder = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}