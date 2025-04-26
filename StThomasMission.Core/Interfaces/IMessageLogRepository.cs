using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IMessageLogRepository : IRepository<MessageLog>
    {
        Task<IEnumerable<MessageLog>> GetByRecipientAsync(string recipient, DateTime? startDate = null);
        Task<IEnumerable<MessageLog>> GetByTypeAsync(MessageType type);
        Task<IEnumerable<MessageLog>> GetByChannelAsync(CommunicationChannel channel);
    }
}