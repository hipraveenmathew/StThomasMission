using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface ICommunicationService
    {
        Task SendMessageAsync(string recipient, string message, CommunicationChannel method, MessageType messageType);
        Task SendAnnouncementAsync(string message, int? wardId = null, CommunicationChannel method = CommunicationChannel.Email);
        Task SendAbsenteeNotificationAsync(int studentId, DateTime date, CommunicationChannel method = CommunicationChannel.Email);
        Task SendFeeReminderAsync(int studentId, string feeDetails, CommunicationChannel method = CommunicationChannel.Email);
        Task SendGroupUpdateAsync(int groupId, string updateMessage, CommunicationChannel method = CommunicationChannel.Email);
        Task<IEnumerable<MessageLog>> GetMessageLogsAsync(string? recipient = null, CommunicationChannel? method = null, MessageType? messageType = null, DateTime? startDate = null);
    }
}