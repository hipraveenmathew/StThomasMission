using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IMassTimingRepository : IRepository<MassTiming>
    {
        Task<IEnumerable<MassTiming>> GetByWeekStartDateAsync(DateTime weekStartDate);
        Task<IEnumerable<MassTiming>> GetByTypeAsync(MassType type);
    }
}