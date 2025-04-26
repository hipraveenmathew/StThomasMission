using StThomasMission.Core.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetAsync(Expression<Func<AuditLog, bool>> predicate);
        IQueryable<AuditLog> GetQueryable(Expression<Func<AuditLog, bool>> predicate); // Added
    }
}