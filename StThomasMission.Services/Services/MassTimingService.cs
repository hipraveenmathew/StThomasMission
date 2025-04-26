using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Services
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

        public async Task AddMassTimingAsync(string day, TimeSpan time, string location, MassType type, DateTime weekStartDate)
        {
            if (string.IsNullOrEmpty(day))
                throw new ArgumentException("Day is required.", nameof(day));
            if (string.IsNullOrEmpty(location))
                throw new ArgumentException("Location is required.", nameof(location));
            if (weekStartDate > DateTime.UtcNow)
                throw new ArgumentException("Week start date cannot be in the future.", nameof(weekStartDate));

            var massTiming = new MassTiming
            {
                Day = day,
                Time = time,
                Location = location,
                Type = type,
                WeekStartDate = weekStartDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            await _unitOfWork.MassTimings.AddAsync(massTiming);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(MassTiming), massTiming.Id.ToString(), $"Added mass timing: {day} {time}");
        }

        public async Task UpdateMassTimingAsync(int massTimingId, string day, TimeSpan time, string location, MassType type, DateTime weekStartDate)
        {
            var massTiming = await GetMassTimingByIdAsync(massTimingId);

            if (string.IsNullOrEmpty(day))
                throw new ArgumentException("Day is required.", nameof(day));
            if (string.IsNullOrEmpty(location))
                throw new ArgumentException("Location is required.", nameof(location));
            if (weekStartDate > DateTime.UtcNow)
                throw new ArgumentException("Week start date cannot be in the future.", nameof(weekStartDate));

            massTiming.Day = day;
            massTiming.Time = time;
            massTiming.Location = location;
            massTiming.Type = type;
            massTiming.WeekStartDate = weekStartDate;
            massTiming.UpdatedAt = DateTime.UtcNow;
            massTiming.UpdatedBy = "System";

            await _unitOfWork.MassTimings.UpdateAsync(massTiming);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(MassTiming), massTimingId.ToString(), $"Updated mass timing: {day} {time}");
        }

        public async Task DeleteMassTimingAsync(int massTimingId)
        {
            var massTiming = await GetMassTimingByIdAsync(massTimingId);
            massTiming.IsDeleted = true;
            massTiming.UpdatedAt = DateTime.UtcNow;
            massTiming.UpdatedBy = "System";

            await _unitOfWork.MassTimings.UpdateAsync(massTiming);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Delete", nameof(MassTiming), massTimingId.ToString(), $"Soft-deleted mass timing: {massTiming.Day} {massTiming.Time}");
        }

        public async Task<IEnumerable<MassTiming>> GetMassTimingsAsync(DateTime? weekStartDate = null)
        {
            return await _unitOfWork.MassTimings.GetByWeekStartDateAsync(weekStartDate ?? DateTime.Today);
        }

        public async Task<MassTiming?> GetMassTimingByIdAsync(int massTimingId)
        {
            var massTiming = await _unitOfWork.MassTimings.GetByIdAsync(massTimingId);
            if (massTiming == null || massTiming.IsDeleted)
                throw new ArgumentException("Mass timing not found.", nameof(massTimingId));
            return massTiming;
        }
    }
}