using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Repository for GroupActivity-specific operations.
    /// </summary>
    public interface IGroupActivityRepository : IRepository<GroupActivity>
    {
        Task<IEnumerable<GroupActivity>> GetByGroupAsync(string group, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<GroupActivity>> GetByStatusAsync(ActivityStatus status);
    }
}