using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class GroupActivityRepository : IGroupActivityRepository
    {
        private readonly StThomasMissionDbContext _context;

        public GroupActivityRepository(StThomasMissionDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GroupActivity>> GetAllAsync()
        {
            return await _context.GroupActivities.ToListAsync();
        }

        public async Task<GroupActivity> GetByIdAsync(int id)
        {
            return await _context.GroupActivities.FindAsync(id);
        }

        public async Task<IEnumerable<GroupActivity>> GetByGroupAsync(string? group = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.GroupActivities.AsQueryable();

            if (!string.IsNullOrEmpty(group))
                query = query.Where(ga => ga.Group == group);

            if (startDate.HasValue)
                query = query.Where(ga => ga.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ga => ga.Date <= endDate.Value);

            return await query.ToListAsync();
        }

        public async Task AddAsync(GroupActivity groupActivity)
        {
            await _context.GroupActivities.AddAsync(groupActivity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(GroupActivity groupActivity)
        {
            _context.GroupActivities.Update(groupActivity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.GroupActivities.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public Task DeleteAsync(GroupActivity entity)
        {
            _context.GroupActivities.Remove(entity);
            return _context.SaveChangesAsync();
        }
    }
}
