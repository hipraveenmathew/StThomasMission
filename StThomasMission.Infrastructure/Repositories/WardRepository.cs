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
    public class WardRepository : Repository<Ward>, IWardRepository
    {
        private readonly StThomasMissionDbContext _context;

        public WardRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Ward?> GetByNameAsync(string name)
        {
            return await _context.Wards
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Name == name);
        }

        public async Task<IEnumerable<Ward>> GetByStatusAsync(bool isActive)
        {
            return await _context.Wards
                .AsNoTracking()
                .Where(w => isActive ? w.Families.Any(f => f.Status != FamilyStatus.Deleted) : !w.Families.Any())
                .ToListAsync();
        }
    }
}