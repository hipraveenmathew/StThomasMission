using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing all forms of communication.
    /// </summary>
    public interface ICommunicationService
    {
        Task SendMessageAsync(string recipient, string message, CommunicationChannel method, MessageType messageType);
        Task SendAnnouncementAsync(string message, string? ward = null, CommunicationChannel method = CommunicationChannel.Email);
        Task SendAbsenteeNotificationAsync(int studentId, DateTime date, CommunicationChannel method = CommunicationChannel.Email);
        Task SendFeeReminderAsync(int studentId, string feeDetails, CommunicationChannel method = CommunicationChannel.Email);
        Task SendGroupUpdateAsync(string groupName, string updateMessage, CommunicationChannel method = CommunicationChannel.Email);
        Task SendAbsenteeNotificationsAsync(string grade, DateTime? date = null, CommunicationChannel method = CommunicationChannel.Email);
        Task<string> GetMessageTemplateAsync(string templateName);
        Task UpdateMessageTemplateAsync(string templateName, string templateContent);
        Task<IEnumerable<MessageLog>> GetMessageLogsAsync(string? recipient = null, CommunicationChannel? method = null, MessageType? messageType = null, DateTime? startDate = null);
    }
}