using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Announcements
                .AsNoTracking()
                .Where(a => a.IsActive && !a.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(a => a.PostedDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.PostedDate <= endDate.Value);

            return await query.ToListAsync();
        }
    }
}