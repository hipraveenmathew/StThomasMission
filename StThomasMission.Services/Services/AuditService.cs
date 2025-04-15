using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Core.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogActionAsync(string userId, string action, string entityName, int entityId, string details)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _unitOfWork._context.AuditLogs.Add(auditLog);
            await _unitOfWork.CompleteAsync();
        }
    }
}