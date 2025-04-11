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

        public async Task<GroupActivity> AddAsync(GroupActivity groupActivity)
        {
            _context.GroupActivities.Add(groupActivity);
            await _context.SaveChangesAsync();
            return groupActivity;
        }

        public async Task<IEnumerable<GroupActivity>> GetByGroupNameAsync(string groupName)
        {
            return await _context.GroupActivities
                .Where(ga => ga.GroupName == groupName)
                .ToListAsync();
        }
    }
}