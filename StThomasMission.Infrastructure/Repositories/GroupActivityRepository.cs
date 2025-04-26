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
    public class GroupActivityRepository : Repository<GroupActivity>, IGroupActivityRepository
    {
        private readonly StThomasMissionDbContext _context;

        public GroupActivityRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GroupActivity>> GetByGroupIdAsync(int groupId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.GroupActivities
                .AsNoTracking()
                .Where(ga => ga.GroupId == groupId && ga.Status != ActivityStatus.Inactive);

            if (startDate.HasValue)
                query = query.Where(ga => ga.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ga => ga.Date <= endDate.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<GroupActivity>> GetByStatusAsync(ActivityStatus status)
        {
            return await _context.GroupActivities
                .AsNoTracking()
                .Where(ga => ga.Status == status)
                .ToListAsync();
        }
    }
}