using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IMassTimingService
    {
        Task AddMassTimingAsync(string day, TimeSpan time, string location, MassType type, DateTime weekStartDate);
        Task UpdateMassTimingAsync(int massTimingId, string day, TimeSpan time, string location, MassType type, DateTime weekStartDate);
        Task DeleteMassTimingAsync(int massTimingId);
        Task<IEnumerable<MassTiming>> GetMassTimingsAsync(DateTime? weekStartDate = null);
        Task<MassTiming?> GetMassTimingByIdAsync(int massTimingId);
    }
}