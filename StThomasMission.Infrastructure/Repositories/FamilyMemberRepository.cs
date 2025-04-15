using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class FamilyMemberRepository : IFamilyMemberRepository
    {
        private readonly StThomasMissionDbContext _context;

        public FamilyMemberRepository(StThomasMissionDbContext context)
        {
            _context = context;
        }

        // IRepository<FamilyMember> Implementation

        public async Task<IEnumerable<FamilyMember>> GetAllAsync()
        {
            return await _context.FamilyMembers.ToListAsync();
        }

        public async Task<FamilyMember> GetByIdAsync(int id)
        {
            return await _context.FamilyMembers.FindAsync(id);
        }

        public async Task AddAsync(FamilyMember familyMember)
        {
            await _context.FamilyMembers.AddAsync(familyMember);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FamilyMember familyMember)
        {
            _context.FamilyMembers.Update(familyMember);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.FamilyMembers.FindAsync(id);
            if (entity != null)
            {
                _context.FamilyMembers.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public Task DeleteAsync(FamilyMember entity)
        {
            _context.FamilyMembers.Remove(entity);
            return _context.SaveChangesAsync();
        }

        // IFamilyMemberRepository Specific Methods

        public async Task<IEnumerable<FamilyMember>> GetByFamilyIdAsync(int familyId)
        {
            return await _context.FamilyMembers
                .Where(m => m.FamilyId == familyId)
                .ToListAsync();
        }
        public async Task<FamilyMember?> GetByUserIdAsync(string userId)
        {
            return await _context.FamilyMembers
                .FirstOrDefaultAsync(m => m.UserId == userId);
        }

    }
}
