using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class FamilyRepository : Repository<Family>, IFamilyRepository
    {
        public FamilyRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<Family?> GetByChurchRegistrationNumberAsync(string churchRegistrationNumber)
        {
            return await _context.Families
                .Include(f => f.Ward)
                .FirstOrDefaultAsync(f => f.ChurchRegistrationNumber == churchRegistrationNumber);
        }

        public async Task<Family?> GetByTemporaryIdAsync(string temporaryId)
        {
            return await _context.Families
                .Include(f => f.Ward)
                .FirstOrDefaultAsync(f => f.TemporaryID == temporaryId);
        }

        public async Task<IEnumerable<Family>> GetByWardAsync(int wardId)
        {
            return await _context.Families
                .Include(f => f.Ward)
                .Where(f => f.WardId == wardId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Family>> GetByStatusAsync(FamilyStatus status)
        {
            return await _context.Families
                .Include(f => f.Ward)
                .Where(f => f.Status == status)
                .ToListAsync();
        }

        public IQueryable<Family> GetQueryable(Expression<Func<Family, bool>> predicate)
        {
            return _context.Families
                .Include(f => f.Ward)
                .Where(predicate)
                .AsQueryable();
        }
        public IQueryable<Family> GetAllQueryable()
        {
            return _context.Families.AsQueryable();
        }
    }
}