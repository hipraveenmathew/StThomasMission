using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class FamilyRepository : Repository<Family>, IRepository<Family>
    {
        private readonly StThomasMissionDbContext _context;

        public FamilyRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Family>> GetByWardAsync(int wardId)
        {
            return await _context.Families
                .AsNoTracking()
                .Where(f => f.WardId == wardId && f.Status != FamilyStatus.Deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Family>> GetByStatusAsync(FamilyStatus status)
        {
            return await _context.Families
                .AsNoTracking()
                .Where(f => f.Status == status)
                .ToListAsync();
        }
    }
}