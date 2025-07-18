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
    public class FamilyMemberRepository : Repository<FamilyMember>, IFamilyMemberRepository
    {
        public FamilyMemberRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IEnumerable<FamilyMemberDto>> GetByFamilyIdAsync(int familyId)
        {
            // The global query filters for FamilyMember and Family handle IsDeleted status.
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.FamilyId == familyId)
                .Select(m => new FamilyMemberDto
                {
                    Id = m.Id,
                    FamilyId = m.FamilyId,
                    UserId = m.UserId,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    Relation = m.Relation,
                    DateOfBirth = m.DateOfBirth,
                    Contact = m.Contact,
                    Email = m.Email
                })
                .OrderBy(m => m.Relation)
                .ThenBy(m => m.DateOfBirth)
                .ToListAsync();
        }

        public async Task<FamilyMemberDto?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.UserId == userId)
                .Select(m => new FamilyMemberDto
                {
                    Id = m.Id,
                    FamilyId = m.FamilyId,
                    UserId = m.UserId,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    Relation = m.Relation,
                    DateOfBirth = m.DateOfBirth,
                    Contact = m.Contact,
                    Email = m.Email
                })
                .FirstOrDefaultAsync();
        }
    }
}