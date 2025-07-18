using SendGrid.Helpers.Errors.Model;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NotFoundException = StThomasMission.Services.Exceptions.NotFoundException;

namespace StThomasMission.Services.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService; // Assuming IAuditService exists

        public AnnouncementService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<IEnumerable<AnnouncementSummaryDto>> GetActiveAnnouncementsAsync()
        {
            return await _unitOfWork.Announcements.GetActiveAnnouncementsAsync();
        }

        public async Task<AnnouncementDetailDto> GetAnnouncementByIdAsync(int announcementId)
        {
            var announcement = await _unitOfWork.Announcements.GetByIdAsync(announcementId);
            if (announcement == null)
            {
                throw new NotFoundException(nameof(Announcement), announcementId);
            }

            // Simple mapping for detail view. AutoMapper could be used in a larger project.
            return new AnnouncementDetailDto
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Description = announcement.Description,
                PostedDate = announcement.PostedDate,
                IsActive = announcement.IsActive
            };
        }

        public async Task<AnnouncementDetailDto> CreateAnnouncementAsync(CreateAnnouncementRequest request, string userId)
        {
            var announcement = new Announcement
            {
                Title = request.Title,
                Description = request.Description,
                PostedDate = request.PostedDate,
                IsActive = request.IsActive,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            var newAnnouncement = await _unitOfWork.Announcements.AddAsync(announcement);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Create", nameof(Announcement), newAnnouncement.Id.ToString(), $"Created announcement: '{newAnnouncement.Title}'");

            return await GetAnnouncementByIdAsync(newAnnouncement.Id);
        }

        public async Task UpdateAnnouncementAsync(int announcementId, UpdateAnnouncementRequest request, string userId)
        {
            var announcement = await _unitOfWork.Announcements.GetByIdAsync(announcementId);
            if (announcement == null)
            {
                throw new NotFoundException(nameof(Announcement), announcementId);
            }

            // Update properties from the request
            announcement.Title = request.Title;
            announcement.Description = request.Description;
            announcement.PostedDate = request.PostedDate;
            announcement.IsActive = request.IsActive;
            announcement.UpdatedBy = userId;
            announcement.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Announcements.UpdateAsync(announcement);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Update", nameof(Announcement), announcementId.ToString(), $"Updated announcement: '{announcement.Title}'");
        }

        public async Task DeleteAnnouncementAsync(int announcementId, string userId)
        {
            var announcement = await _unitOfWork.Announcements.GetByIdAsync(announcementId);
            if (announcement == null)
            {
                throw new NotFoundException(nameof(Announcement), announcementId);
            }

            // This is a soft delete
            announcement.IsDeleted = true;
            announcement.IsActive = false; // Also mark as inactive
            announcement.UpdatedBy = userId;
            announcement.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Announcements.UpdateAsync(announcement);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Delete", nameof(Announcement), announcementId.ToString(), $"Soft-deleted announcement: '{announcement.Title}'");
        }
    }
}