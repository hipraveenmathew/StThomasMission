using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        // Change the 'entityId' parameter from int to string
        public async Task LogActionAsync(string userId, string action, string entityName, string entityId, string details)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId, // This is now a string
                Details = details,
                Timestamp = DateTime.UtcNow,
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IPaginatedList<AuditLogDto>> GetLogsAsync(
            int pageNumber,
            int pageSize,
            string? userId = null,
            string? entityName = null,
            string? sortOrder = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            return await _unitOfWork.AuditLogs.GetLogsPaginatedAsync(
                pageNumber,
                pageSize,
                userId,
                entityName,
                sortOrder,
                startDate,
                endDate);
        }
    }
}