using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class MigrationLogService : IMigrationLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public MigrationLogService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task LogFamilyMigrationAsync(LogMigrationRequest request, string userId)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(request.FamilyId);
            if (family == null)
            {
                throw new NotFoundException(nameof(Family), request.FamilyId);
            }

            var migrationLog = new MigrationLog
            {
                FamilyId = request.FamilyId,
                MigratedTo = request.MigratedTo,
                MigrationDate = request.MigrationDate,
                CreatedBy = userId
            };

            await _unitOfWork.MigrationLogs.AddAsync(migrationLog);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Create", nameof(MigrationLog), migrationLog.Id.ToString(), $"Logged migration for family '{family.FamilyName}' to {request.MigratedTo}.");
        }

        public async Task<IPaginatedList<MigrationLogDto>> GetMigrationLogsAsync(
            int pageNumber,
            int pageSize,
            int? familyId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            // This is a clean pass-through to the repository's powerful search method.
            return await _unitOfWork.MigrationLogs.SearchMigrationLogsPaginatedAsync(
                pageNumber,
                pageSize,
                familyId,
                startDate,
                endDate);
        }
    }
}