using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using StThomasMission.Infrastructure.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class GroupActivityRepository : Repository<GroupActivity>, IGroupActivityRepository
    {
        public GroupActivityRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IEnumerable<GroupActivityDto>> GetByGroupIdAsync(int groupId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(ga => ga.GroupId == groupId);

            if (startDate.HasValue)
            {
                query = query.Where(ga => ga.Date.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(ga => ga.Date.Date <= endDate.Value.Date);
            }

            return await query
                .Select(ga => new GroupActivityDto
                {
                    Id = ga.Id,
                    Name = ga.Name,
                    Description = ga.Description,
                    Date = ga.Date,
                    GroupId = ga.GroupId,
                    GroupName = ga.Group.Name, // Projecting the name
                    Points = ga.Points,
                    Status = ga.Status
                })
                .OrderByDescending(ga => ga.Date)
                .ToListAsync();
        }
        // Add this new method to the GroupActivityRepository class

        public async Task<List<GroupActivityDto>> GetUpcomingActivitiesAsync(DateTime fromDate, int take)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(ga => ga.Date >= fromDate.Date)
                .OrderBy(ga => ga.Date)
                .Take(take)
                .Select(ga => new GroupActivityDto
                {
                    Id = ga.Id,
                    Name = ga.Name,
                    Date = ga.Date,
                    GroupName = ga.Group.Name,
                    Points = ga.Points
                })
                .ToListAsync();
        }

        public async Task<IPaginatedList<GroupActivityDto>> GetActivitiesByStatusPaginatedAsync(int pageNumber, int pageSize, ActivityStatus status)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(ga => ga.Status == status);

            var dtoQuery = query.Select(ga => new GroupActivityDto
            {
                Id = ga.Id,
                Name = ga.Name,
                Description = ga.Description,
                Date = ga.Date,
                GroupId = ga.GroupId,
                GroupName = ga.Group.Name,
                Points = ga.Points,
                Status = ga.Status
            });

            return await PaginatedList<GroupActivityDto>.CreateAsync(
                dtoQuery.OrderByDescending(ga => ga.Date),
                pageNumber,
                pageSize);
        }
    }
}