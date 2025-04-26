using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public AnnouncementService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task AddAnnouncementAsync(string title, string description, DateTime postedDate)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentException("Title is required.", nameof(title));
            if (string.IsNullOrEmpty(description))
                throw new ArgumentException("Description is required.", nameof(description));
            if (postedDate > DateTime.UtcNow)
                throw new ArgumentException("Posted date cannot be in the future.", nameof(postedDate));

            var announcement = new Announcement
            {
                Title = title,
                Description = description,
                PostedDate = postedDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            await _unitOfWork.Announcements.AddAsync(announcement);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(Announcement), announcement.Id.ToString(), $"Added announcement: {title}");
        }

        public async Task UpdateAnnouncementAsync(int announcementId, string title, string description, DateTime postedDate, bool isActive)
        {
            var announcement = await GetAnnouncementByIdAsync(announcementId);

            if (string.IsNullOrEmpty(title))
                throw new ArgumentException("Title is required.", nameof(title));
            if (string.IsNullOrEmpty(description))
                throw new ArgumentException("Description is required.", nameof(description));
            if (postedDate > DateTime.UtcNow)
                throw new ArgumentException("Posted date cannot be in the future.", nameof(postedDate));

            announcement.Title = title;
            announcement.Description = description;
            announcement.PostedDate = postedDate;
            announcement.IsActive = isActive;
            announcement.UpdatedAt = DateTime.UtcNow;
            announcement.UpdatedBy = "System";

            await _unitOfWork.Announcements.UpdateAsync(announcement);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Announcement), announcementId.ToString(), $"Updated announcement: {title}");
        }

        public async Task DeleteAnnouncementAsync(int announcementId)
        {
            var announcement = await GetAnnouncementByIdAsync(announcementId);
            announcement.IsDeleted = true;
            announcement.UpdatedAt = DateTime.UtcNow;
            announcement.UpdatedBy = "System";

            await _unitOfWork.Announcements.UpdateAsync(announcement);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Delete", nameof(Announcement), announcementId.ToString(), $"Soft-deleted announcement: {announcement.Title}");
        }

        public async Task<IEnumerable<Announcement>> GetAnnouncementsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _unitOfWork.Announcements.GetActiveAnnouncementsAsync(startDate, endDate);
        }

        public async Task<Announcement?> GetAnnouncementByIdAsync(int announcementId)
        {
            var announcement = await _unitOfWork.Announcements.GetByIdAsync(announcementId);
            if (announcement == null || announcement.IsDeleted)
                throw new ArgumentException("Announcement not found.", nameof(announcementId));
            return announcement;
        }
    }
}