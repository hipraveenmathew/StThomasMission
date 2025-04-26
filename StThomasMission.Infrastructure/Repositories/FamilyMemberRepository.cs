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
    public class FamilyMemberRepository : Repository<FamilyMember>, IFamilyMemberRepository
    {
        private readonly StThomasMissionDbContext _context;

        public FamilyMemberRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FamilyMember>> GetByFamilyIdAsync(int familyId)
        {
            return await _context.FamilyMembers
                .AsNoTracking()
                .Where(m => m.FamilyId == familyId && m.Family.Status != FamilyStatus.Deleted)
                .ToListAsync();
        }

        public async Task<FamilyMember?> GetByUserIdAsync(string userId)
        {
            return await _context.FamilyMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId && m.Family.Status != FamilyStatus.Deleted);
        }
    }
}