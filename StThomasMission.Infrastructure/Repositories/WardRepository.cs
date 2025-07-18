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
    public class WardRepository : Repository<Ward>, IWardRepository
    {
        public WardRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<WardDetailDto?> GetByNameAsync(string name)
        {
            // The global query filter handles the IsDeleted status.
            return await _dbSet
                .AsNoTracking()
                .Where(w => w.Name == name)
                .Select(w => new WardDetailDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    // Efficiently count related families without loading them.
                    FamilyCount = w.Families.Count()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<WardDetailDto>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .OrderBy(w => w.Name)
                .Select(w => new WardDetailDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    FamilyCount = w.Families.Count()
                })
                .ToListAsync();
        }
    }
}