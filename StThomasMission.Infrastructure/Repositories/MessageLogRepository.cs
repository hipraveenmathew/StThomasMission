using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using StThomasMission.Infrastructure.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class MessageLogRepository : Repository<MessageLog>, IMessageLogRepository
    {
        public MessageLogRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IPaginatedList<MessageLogDto>> SearchLogsPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? recipient = null,
            MessageType? messageType = null,
            CommunicationChannel? channel = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _dbSet.AsNoTracking();

            // Apply filters conditionally
            if (!string.IsNullOrWhiteSpace(recipient))
            {
                query = query.Where(ml => ml.Recipient.Contains(recipient));
            }

            if (messageType.HasValue)
            {
                query = query.Where(ml => ml.MessageType == messageType.Value);
            }

            if (channel.HasValue)
            {
                query = query.Where(ml => ml.Method == channel.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(ml => ml.SentAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(ml => ml.SentAt < endDate.Value.AddDays(1));
            }

            var dtoQuery = query.Select(ml => new MessageLogDto
            {
                Id = ml.Id,
                Recipient = ml.Recipient,
                Message = ml.Message,
                Method = ml.Method,
                MessageType = ml.MessageType,
                SentAt = ml.SentAt,
                Status = ml.Status,
                ResponseDetails = ml.ResponseDetails,
                SentBy = ml.SentBy
            });

            return await PaginatedList<MessageLogDto>.CreateAsync(
                dtoQuery.OrderByDescending(log => log.SentAt),
                pageNumber,
                pageSize);
        }

        public async Task LogMessageAsync(string recipient, string message, CommunicationChannel channel, MessageType messageType, string status, string? responseDetails, string sentByUserId)
        {
            var messageLog = new MessageLog
            {
                Recipient = recipient,
                Message = message,
                Method = channel,
                MessageType = messageType,
                Status = status,
                ResponseDetails = responseDetails,
                SentBy = sentByUserId,
                SentAt = DateTime.UtcNow
            };

            // This just adds the entity to the context. The Unit of Work will save it.
            await _dbSet.AddAsync(messageLog);
        }
    }
}