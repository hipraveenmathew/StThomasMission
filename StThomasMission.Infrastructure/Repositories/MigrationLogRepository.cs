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
    public class MigrationLogRepository : Repository<MigrationLog>, IMigrationLogRepository
    {
        private readonly StThomasMissionDbContext _context;

        public MigrationLogRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MigrationLog>> GetByFamilyIdAsync(int familyId)
        {
            return await _context.MigrationLogs
                .AsNoTracking()
                .Where(ml => ml.FamilyId == familyId && ml.Family.Status != FamilyStatus.Deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<MigrationLog>> GetByMigrationDateAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.MigrationLogs
                .AsNoTracking()
                .Where(ml => ml.MigrationDate >= startDate && ml.MigrationDate <= endDate && ml.Family.Status != FamilyStatus.Deleted)
                .ToListAsync();
        }
    }
}