using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class FamilyRepository : IFamilyRepository
    {
        private readonly StThomasMissionDbContext _context;

        public FamilyRepository(StThomasMissionDbContext context)
        {
            _context = context;
        }

        // IRepository<Family> Implementation

        public async Task<IEnumerable<Family>> GetAllAsync()
        {
            return await _context.Families.ToListAsync();
        }

        public async Task<Family> GetByIdAsync(int id)
        {
            return await _context.Families.FindAsync(id);
        }

        public async Task AddAsync(Family family)
        {
            await _context.Families.AddAsync(family);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Family family)
        {
            _context.Families.Update(family);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Families.FindAsync(id);
            if (entity != null)
            {
                _context.Families.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public Task DeleteAsync(Family entity)
        {
            _context.Families.Remove(entity);
            return _context.SaveChangesAsync();
        }

        // IFamilyRepository Specific Methods

        public async Task<IEnumerable<Family>> GetByWardAsync(string ward)
        {
            return await _context.Families
                .Where(f => f.Ward == ward)
                .ToListAsync();
        }

        public async Task<IEnumerable<Family>> GetByStatusAsync(string status)
        {
            if (!Enum.TryParse<FamilyStatus>(status, true, out var parsedStatus))
                throw new ArgumentException("Invalid family status.", nameof(status));

            return await _context.Families
                .Where(f => f.Status == parsedStatus)
                .ToListAsync();
        }
    }
}
