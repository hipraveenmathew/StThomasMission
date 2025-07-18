using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using StThomasMission.Infrastructure.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class MigrationLogRepository : Repository<MigrationLog>, IMigrationLogRepository
    {
        public MigrationLogRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IPaginatedList<MigrationLogDto>> SearchMigrationLogsPaginatedAsync(int pageNumber, int pageSize, int? familyId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.AsNoTracking();

            if (familyId.HasValue)
            {
                query = query.Where(ml => ml.FamilyId == familyId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(ml => ml.MigrationDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(ml => ml.MigrationDate.Date <= endDate.Value.Date);
            }

            var dtoQuery = query.Select(ml => new MigrationLogDto
            {
                Id = ml.Id,
                FamilyId = ml.FamilyId,
                FamilyName = ml.Family.FamilyName,
                ChurchRegistrationNumber = ml.Family.ChurchRegistrationNumber,
                MigratedTo = ml.MigratedTo,
                MigrationDate = ml.MigrationDate,
                CreatedBy = ml.CreatedBy
            });

            return await PaginatedList<MigrationLogDto>.CreateAsync(
                dtoQuery.OrderByDescending(log => log.MigrationDate),
                pageNumber,
                pageSize);
        }
    }
}