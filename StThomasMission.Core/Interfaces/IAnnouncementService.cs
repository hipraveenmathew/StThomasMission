using StThomasMission.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<AnnouncementSummaryDto>> GetActiveAnnouncementsAsync();

        Task<AnnouncementDetailDto> GetAnnouncementByIdAsync(int announcementId);

        Task<AnnouncementDetailDto> CreateAnnouncementAsync(CreateAnnouncementRequest request, string userId);

        Task UpdateAnnouncementAsync(int announcementId, UpdateAnnouncementRequest request, string userId);

        Task DeleteAnnouncementAsync(int announcementId, string userId);
    }
}