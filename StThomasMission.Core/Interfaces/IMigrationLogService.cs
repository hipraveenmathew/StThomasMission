using StThomasMission.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IMigrationLogService
    {
        Task AddMigrationLogAsync(int familyId, string migratedTo, DateTime migrationDate);
        Task<IEnumerable<MigrationLog>> GetMigrationLogsByFamilyAsync(int familyId);
        Task<IEnumerable<MigrationLog>> GetMigrationLogsByDateAsync(DateTime startDate, DateTime endDate);
    }
}