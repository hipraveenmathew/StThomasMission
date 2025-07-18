using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class GroupRepository : Repository<Group>, IGroupRepository
    {
        public GroupRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<GroupDetailDto?> GetByNameAsync(string name)
        {
            // The global query filter on Group handles IsDeleted status automatically.
            return await _dbSet
                .AsNoTracking()
                .Where(g => g.Name == name)
                .Select(g => new GroupDetailDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    // Efficiently count related students without loading them
                    StudentCount = g.Students.Count()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<GroupDetailDto>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .OrderBy(g => g.Name)
                .Select(g => new GroupDetailDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    StudentCount = g.Students.Count()
                })
                .ToListAsync();
        }
    }
}