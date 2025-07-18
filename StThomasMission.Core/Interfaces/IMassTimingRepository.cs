using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IMassTimingRepository : IRepository<MassTiming>
    {
        Task<IEnumerable<MassTimingDto>> GetByWeekStartDateAsync(DateTime weekStartDate);

        Task<IEnumerable<MassTimingDto>> GetCurrentAndUpcomingMassesAsync();
    }
}