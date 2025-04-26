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
    public class GroupRepository : Repository<Group>, IGroupRepository
    {
        private readonly StThomasMissionDbContext _context;

        public GroupRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Group?> GetByNameAsync(string name)
        {
            return await _context.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Name == name);
        }

        public async Task<IEnumerable<Group>> GetAllActiveAsync()
        {
            return await _context.Groups
                .AsNoTracking()
                .Where(g => g.Students.Any(s => s.Status != StudentStatus.Deleted))
                .ToListAsync();
        }
    }
}