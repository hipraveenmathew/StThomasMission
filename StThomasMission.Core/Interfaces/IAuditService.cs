using StThomasMission.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(string userId, string action, string entityName, string entityId, string details);
        IQueryable<AuditLog> GetAuditLogsQueryable(string? entityName = null, DateTime? startDate = null, DateTime? endDate = null); // Changed to IQueryable
    }
}