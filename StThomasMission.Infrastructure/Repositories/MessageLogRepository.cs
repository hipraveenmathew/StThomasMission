using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class MessageLogRepository : Repository<MessageLog>, IMessageLogRepository
    {
        private readonly StThomasMissionDbContext _context;

        public MessageLogRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MessageLog>> GetByRecipientAsync(string recipient, DateTime? startDate = null)
        {
            var query = _context.MessageLogs
                .AsNoTracking()
                .Where(ml => ml.Recipient == recipient);

            if (startDate.HasValue)
                query = query.Where(ml => ml.SentAt >= startDate.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<MessageLog>> GetByTypeAsync(MessageType type)
        {
            return await _context.MessageLogs
                .AsNoTracking()
                .Where(ml => ml.MessageType == type)
                .ToListAsync();
        }

        public async Task<IEnumerable<MessageLog>> GetByChannelAsync(CommunicationChannel channel)
        {
            return await _context.MessageLogs
                .AsNoTracking()
                .Where(ml => ml.Method == channel)
                .ToListAsync();
        }
        public IQueryable<MessageLog> GetQueryable(Expression<Func<MessageLog, bool>> predicate)
        {
            return _context.Set<MessageLog>()
                .Where(predicate)
                .AsQueryable();
        }
    }
}