using StThomasMission.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAnnouncementService
    {
        Task AddAnnouncementAsync(string title, string description, DateTime postedDate);
        Task UpdateAnnouncementAsync(int announcementId, string title, string description, DateTime postedDate, bool isActive);
        Task DeleteAnnouncementAsync(int announcementId);
        Task<IEnumerable<Announcement>> GetAnnouncementsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Announcement?> GetAnnouncementByIdAsync(int announcementId);
    }
}