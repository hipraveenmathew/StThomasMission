using StThomasMission.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IMassTimingService
    {
        Task<MassTimingDto> GetMassTimingByIdAsync(int massTimingId);

        Task<IEnumerable<MassTimingDto>> GetMassesForWeekAsync(DateTime weekStartDate);

        Task<IEnumerable<MassTimingDto>> GetCurrentAndUpcomingMassesAsync();

        Task<MassTimingDto> AddMassTimingAsync(CreateMassTimingRequest request, string userId);

        Task UpdateMassTimingAsync(int massTimingId, UpdateMassTimingRequest request, string userId);

        Task DeleteMassTimingAsync(int massTimingId, string userId);
    }
}