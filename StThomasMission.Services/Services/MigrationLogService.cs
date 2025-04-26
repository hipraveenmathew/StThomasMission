using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class MigrationLogService : IMigrationLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFamilyService _familyService;
        private readonly IAuditService _auditService;

        public MigrationLogService(IUnitOfWork unitOfWork, IFamilyService familyService, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _familyService = familyService;
            _auditService = auditService;
        }

        public async Task AddMigrationLogAsync(int familyId, string migratedTo, DateTime migrationDate)
        {
            if (string.IsNullOrEmpty(migratedTo))
                throw new ArgumentException("Migration target is required.", nameof(migratedTo));
            if (migrationDate > DateTime.UtcNow)
                throw new ArgumentException("Migration date cannot be in the future.", nameof(migrationDate));

            await _familyService.GetFamilyByIdAsync(familyId);

            var migrationLog = new MigrationLog
            {
                FamilyId = familyId,
                MigratedTo = migratedTo,
                MigrationDate = migrationDate,
                CreatedBy = "System"
            };

            await _unitOfWork.MigrationLogs.AddAsync(migrationLog);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(MigrationLog), migrationLog.Id.ToString(), $"Added migration log for family {familyId} to {migratedTo}");
        }

        public async Task<IEnumerable<MigrationLog>> GetMigrationLogsByFamilyAsync(int familyId)
        {
            await _familyService.GetFamilyByIdAsync(familyId);
            return await _unitOfWork.MigrationLogs.GetByFamilyIdAsync(familyId);
        }

        public async Task<IEnumerable<MigrationLog>> GetMigrationLogsByDateAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date.", nameof(startDate));
            if (endDate > DateTime.UtcNow)
                throw new ArgumentException("End date cannot be in the future.", nameof(endDate));

            return await _unitOfWork.MigrationLogs.GetByMigrationDateAsync(startDate, endDate);
        }
    }
}