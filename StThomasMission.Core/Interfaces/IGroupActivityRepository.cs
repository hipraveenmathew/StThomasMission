using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IGroupActivityRepository : IRepository<GroupActivity>
    {
        Task<IEnumerable<GroupActivityDto>> GetByGroupIdAsync(int groupId, DateTime? startDate = null, DateTime? endDate = null);

        Task<IPaginatedList<GroupActivityDto>> GetActivitiesByStatusPaginatedAsync(int pageNumber, int pageSize, ActivityStatus status);
        Task<List<GroupActivityDto>> GetUpcomingActivitiesAsync(DateTime fromDate, int take);

    }
}