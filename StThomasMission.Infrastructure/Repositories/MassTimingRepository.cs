using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
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
        private readonly StThomasMissionDbContext _context;

        public MassTimingRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MassTiming>> GetByWeekStartDateAsync(DateTime weekStartDate)
        {
            return await _context.MassTimings
                .AsNoTracking()
                .Where(mt => mt.WeekStartDate == weekStartDate && !mt.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<MassTiming>> GetByTypeAsync(MassType type)
        {
            return await _context.MassTimings
                .AsNoTracking()
                .Where(mt => mt.Type == type && !mt.IsDeleted)
                .ToListAsync();
        }
    }
}