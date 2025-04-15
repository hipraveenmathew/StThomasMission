using StThomasMission.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IGroupActivityRepository : IRepository<GroupActivity>
    {
        Task<IEnumerable<GroupActivity>> GetByGroupAsync(string group, DateTime? startDate = null, DateTime? endDate = null);
    }
}