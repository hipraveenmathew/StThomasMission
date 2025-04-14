using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;

namespace StThomasMission.Infrastructure.Repositories
{
    public class GroupActivityRepository : IGroupActivityRepository
    {
        private readonly StThomasMissionDbContext _context;

        public GroupActivityRepository(StThomasMissionDbContext context)
        {
            _context = context;
        }

        public async Task<GroupActivity> GetByIdAsync(int id)
        {
            return await _context.GroupActivities.FindAsync(id);
        }

        public async Task<IEnumerable<GroupActivity>> GetAllAsync()
        {
            return await _context.GroupActivities.ToListAsync();
        }

        public async Task<IEnumerable<GroupActivity>> GetByGroupAsync(string group)
        {
            return await _context.GroupActivities
                .Where(ga => ga.Group == group)
                .ToListAsync();
        }

        public async Task AddAsync(GroupActivity groupActivity)
        {
            await _context.GroupActivities.AddAsync(groupActivity);
        }

        public async Task UpdateAsync(GroupActivity groupActivity)
        {
            _context.GroupActivities.Update(groupActivity);
        }

        public async Task DeleteAsync(int id)
        {
            var groupActivity = await GetByIdAsync(id);
            if (groupActivity != null)
            {
                _context.GroupActivities.Remove(groupActivity);
            }
        }
    }
}