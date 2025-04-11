using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;

namespace StThomasMission.Infrastructure.Repositories
{
    public class FamilyRepository : IFamilyRepository
    {
        private readonly StThomasMissionDbContext _context;

        public FamilyRepository(StThomasMissionDbContext context)
        {
            _context = context;
        }

        public async Task<Family> AddAsync(Family family)
        {
            _context.Families.Add(family);
            await _context.SaveChangesAsync();
            return family;
        }

        public async Task<Family?> GetByIdAsync(int id)
        {
            return await _context.Families
                .Include(f => f.Members)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task UpdateAsync(Family family)
        {
            _context.Families.Update(family);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var family = await GetByIdAsync(id);
            if (family != null)
            {
                _context.Families.Remove(family);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Family>> GetAllAsync()
        {
            return await _context.Families
                .Include(f => f.Members)
                .ToListAsync();
        }
    }
}