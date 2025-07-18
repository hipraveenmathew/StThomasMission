using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class MassTimingRepository : Repository<MassTiming>, IMassTimingRepository
    {
        public MassTimingRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IEnumerable<MassTimingDto>> GetByWeekStartDateAsync(DateTime weekStartDate)
        {
            // The global query filter handles the IsDeleted status.
            return await _dbSet
                .AsNoTracking()
                .Where(mt => mt.WeekStartDate.Date == weekStartDate.Date)
                .Select(mt => new MassTimingDto
                {
                    Id = mt.Id,
                    Day = mt.Day,
                    Time = mt.Time,
                    Location = mt.Location,
                    Type = mt.Type
                })
                .OrderBy(mt => mt.Time)
                .ToListAsync();
        }

        public async Task<IEnumerable<MassTimingDto>> GetCurrentAndUpcomingMassesAsync()
        {
            // Calculate the start of the current week (assuming Monday).
            var today = DateTime.UtcNow.Date;
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            var startOfCurrentWeek = today.AddDays(-daysUntilMonday);

            return await _dbSet
                .AsNoTracking()
                .Where(mt => mt.WeekStartDate.Date >= startOfCurrentWeek)
                 .Select(mt => new MassTimingDto
                 {
                     Id = mt.Id,
                     Day = mt.Day,
                     Time = mt.Time,
                     Location = mt.Location,
                     Type = mt.Type,
                     WeekStartDate = mt.WeekStartDate // Include for sorting
                 })
                .OrderBy(mt => mt.WeekStartDate)
                .ThenBy(mt => mt.Time)
                .ToListAsync();
        }
    }
}