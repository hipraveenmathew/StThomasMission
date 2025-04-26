using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IEnumerable<AuditLog>> GetAsync(Expression<Func<AuditLog, bool>> predicate)
        {
            return await _context.Set<AuditLog>()
                .Where(predicate)
                .ToListAsync();
        }

        public IQueryable<AuditLog> GetQueryable(Expression<Func<AuditLog, bool>> predicate)
        {
            return _context.Set<AuditLog>()
                .Where(predicate)
                .AsQueryable();
        }
    }
}