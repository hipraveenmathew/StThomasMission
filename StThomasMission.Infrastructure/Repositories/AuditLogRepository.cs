using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class AuditLogRepository : Repository<AuditLog>, IRepository<AuditLog>
    {
        private readonly StThomasMissionDbContext _context;

        public AuditLogRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }
    }
}