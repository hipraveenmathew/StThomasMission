using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogActionAsync(string userId, string action, string entityName, string entityId, string details)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required.", nameof(userId));
            if (string.IsNullOrEmpty(action))
                throw new ArgumentException("Action is required.", nameof(action));
            if (string.IsNullOrEmpty(entityName))
                throw new ArgumentException("Entity name is required.", nameof(entityName));
            if (string.IsNullOrEmpty(entityId))
                throw new ArgumentException("Entity ID is required.", nameof(entityId));
            if (string.IsNullOrEmpty(details))
                throw new ArgumentException("Details are required.", nameof(details));

            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details,
                Timestamp = DateTime.UtcNow,
                PerformedBy = userId
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();
        }

        public IQueryable<AuditLog> GetAuditLogsQueryable(string? entityName = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            return _unitOfWork.AuditLogs.GetQueryable(log =>
                (entityName == null || log.EntityName == entityName) &&
                (startDate == null || log.Timestamp >= startDate) &&
                (endDate == null || log.Timestamp <= endDate));
        }
    }
}