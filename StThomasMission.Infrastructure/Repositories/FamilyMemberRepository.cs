using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;

namespace StThomasMission.Infrastructure.Repositories
{
    public class FamilyMemberRepository : Repository<FamilyMember>, IFamilyMemberRepository
    {
        public FamilyMemberRepository(SThomasMissionDbContext context) : base(context) { }

        public async Task<FamilyMember> GetByIdAsync(int id)
        {
            return await _context.FamilyMembers
                .Include(fm => fm.Family)
                .FirstOrDefaultAsync(fm => fm.Id == id);
        }
        public async Task<FamilyMember> GetByUserIdAsync(string userId)
        {
            return await _context.FamilyMembers
                .FirstOrDefaultAsync(fm => fm.UserId == userId);
        }
    }
}