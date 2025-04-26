using StThomasMission.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IMigrationLogRepository : IRepository<MigrationLog>
    {
        Task<IEnumerable<MigrationLog>> GetByFamilyIdAsync(int familyId);
        Task<IEnumerable<MigrationLog>> GetByMigrationDateAsync(DateTime startDate, DateTime endDate);
    }
}