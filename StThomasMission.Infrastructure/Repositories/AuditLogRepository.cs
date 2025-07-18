using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using StThomasMission.Infrastructure.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IPaginatedList<AuditLogDto>> GetLogsPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? userId = null,
            string? entityName = null,
            string? sortOrder = null, // The parameter is now used
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _dbSet.AsNoTracking();

            // Apply filters conditionally
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(log => log.UserId == userId);
            }

            if (!string.IsNullOrEmpty(entityName))
            {
                query = query.Where(log => log.EntityName == entityName);
            }

            if (startDate.HasValue)
            {
                query = query.Where(log => log.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.Timestamp < endDate.Value.AddDays(1));
            }

            var dtoQuery = query.Select(log => new AuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                Username = _context.Users.Where(u => u.Id == log.UserId).Select(u => u.UserName).FirstOrDefault() ?? "Unknown",
                Action = log.Action,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                Details = log.Details,
                Timestamp = log.Timestamp,
                IpAddress = log.IpAddress,
                PerformedBy = log.PerformedBy
            });

            // Apply sorting logic dynamically based on the sortOrder parameter
            dtoQuery = sortOrder switch
            {
                "Timestamp_desc" => dtoQuery.OrderByDescending(l => l.Timestamp),
                "Username" => dtoQuery.OrderBy(l => l.Username),
                "Username_desc" => dtoQuery.OrderByDescending(l => l.Username),
                "Action" => dtoQuery.OrderBy(l => l.Action),
                "Action_desc" => dtoQuery.OrderByDescending(l => l.Action),
                _ => dtoQuery.OrderByDescending(l => l.Timestamp), // Default sort
            };

            return await PaginatedList<AuditLogDto>.CreateAsync(dtoQuery, pageNumber, pageSize);
        }
    }
}