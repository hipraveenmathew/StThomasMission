using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IGroupActivityRepository : IRepository<GroupActivity>
    {
        Task<IEnumerable<GroupActivity>> GetByGroupIdAsync(int groupId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<GroupActivity>> GetByStatusAsync(ActivityStatus status);
    }
}