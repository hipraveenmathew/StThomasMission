using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IMessageLogRepository : IRepository<MessageLog>
    {
        Task<IPaginatedList<MessageLogDto>> SearchLogsPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? recipient = null,
            MessageType? messageType = null,
            CommunicationChannel? channel = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
        Task LogMessageAsync(string recipient, string message, CommunicationChannel channel, MessageType messageType, string status, string? responseDetails, string sentByUserId);

    }

}