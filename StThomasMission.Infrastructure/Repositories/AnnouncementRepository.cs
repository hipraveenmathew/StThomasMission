using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs; // Assuming DTOs are in this namespace
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class AnnouncementRepository : Repository<Announcement>, IAnnouncementRepository
    {
        private readonly StThomasMissionDbContext _context;

        public AnnouncementRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all active announcements.
        /// </summary>
        /// <returns>A collection of announcement summary DTOs.</returns>
        // Replace the existing GetActiveAnnouncementsAsync method with this new version

        public async Task<IEnumerable<AnnouncementSummaryDto>> GetActiveAnnouncementsAsync()
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(a => a.IsActive);

            var dtoQuery = query.Select(a => new AnnouncementSummaryDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                PostedDate = a.PostedDate,
                IsActive = a.IsActive,
                // Join with the Users table to get the full name
                AuthorName = _context.Users
                                     .Where(u => u.Id == a.CreatedBy)
                                     .Select(u => u.FullName)
                                     .FirstOrDefault() ?? "System"
            });

            return await dtoQuery
                .OrderByDescending(a => a.PostedDate)
                .ToListAsync();
        }
    }
}