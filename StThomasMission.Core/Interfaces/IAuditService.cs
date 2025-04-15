using StThomasMission.Core.Entities;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(string userId, string action, string entityName, int entityId, string details);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? entityName = null, DateTime? startDate = null, DateTime? endDate = null);
    }
}