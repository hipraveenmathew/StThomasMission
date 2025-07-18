using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAnnouncementRepository : IRepository<Announcement>
    {
        Task<IEnumerable<AnnouncementSummaryDto>> GetActiveAnnouncementsAsync();
    }
}