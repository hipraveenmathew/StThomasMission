using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class MassTimingService : IMassTimingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public MassTimingService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<MassTimingDto> GetMassTimingByIdAsync(int massTimingId)
        {
            var massTiming = await _unitOfWork.MassTimings.GetByIdAsync(massTimingId);
            if (massTiming == null)
            {
                throw new NotFoundException(nameof(MassTiming), massTimingId);
            }
            // Map to DTO
            return new MassTimingDto
            {
                Id = massTiming.Id,
                Day = massTiming.Day,
                Time = massTiming.Time,
                Location = massTiming.Location,
                Type = massTiming.Type
            };
        }

        public async Task<IEnumerable<MassTimingDto>> GetMassesForWeekAsync(DateTime weekStartDate)
        {
            return await _unitOfWork.MassTimings.GetByWeekStartDateAsync(weekStartDate);
        }

        public async Task<IEnumerable<MassTimingDto>> GetCurrentAndUpcomingMassesAsync()
        {
            return await _unitOfWork.MassTimings.GetCurrentAndUpcomingMassesAsync();
        }

        public async Task<MassTimingDto> AddMassTimingAsync(CreateMassTimingRequest request, string userId)
        {
            var massTiming = new MassTiming
            {
                Day = request.Day,
                Time = request.Time,
                Location = request.Location,
                Type = request.Type,
                WeekStartDate = request.WeekStartDate.Date,
                CreatedBy = userId
            };

            var newMassTiming = await _unitOfWork.MassTimings.AddAsync(massTiming);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Create", nameof(MassTiming), newMassTiming.Id.ToString(), $"Added mass timing: {newMassTiming.Day} {newMassTiming.Time}.");

            return await GetMassTimingByIdAsync(newMassTiming.Id);
        }

        public async Task UpdateMassTimingAsync(int massTimingId, UpdateMassTimingRequest request, string userId)
        {
            var massTiming = await _unitOfWork.MassTimings.GetByIdAsync(massTimingId);
            if (massTiming == null)
            {
                throw new NotFoundException(nameof(MassTiming), massTimingId);
            }

            massTiming.Day = request.Day;
            massTiming.Time = request.Time;
            massTiming.Location = request.Location;
            massTiming.Type = request.Type;
            massTiming.WeekStartDate = request.WeekStartDate.Date;
            massTiming.UpdatedBy = userId;
            massTiming.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.MassTimings.UpdateAsync(massTiming);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Update", nameof(MassTiming), massTimingId.ToString(), $"Updated mass timing: {massTiming.Day} {massTiming.Time}.");
        }

        public async Task DeleteMassTimingAsync(int massTimingId, string userId)
        {
            var massTiming = await _unitOfWork.MassTimings.GetByIdAsync(massTimingId);
            if (massTiming == null)
            {
                throw new NotFoundException(nameof(MassTiming), massTimingId);
            }

            massTiming.IsDeleted = true;
            massTiming.UpdatedBy = userId;
            massTiming.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.MassTimings.UpdateAsync(massTiming);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Delete", nameof(MassTiming), massTimingId.ToString(), $"Soft-deleted mass timing: {massTiming.Day} {massTiming.Time}.");
        }
    }
}