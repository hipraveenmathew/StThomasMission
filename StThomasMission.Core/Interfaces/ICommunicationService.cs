﻿using StThomasMission.Core.Entities;
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
        Task SendMessageAsync(string recipient, string message, string method, string messageType);
        Task SendAnnouncementAsync(string message, string? ward = null, string method = "Email");
        Task SendAbsenteeNotificationAsync(int studentId, DateTime date, string method = "Email");
        Task SendFeeReminderAsync(int studentId, string feeDetails, string method = "Email");
        Task SendGroupUpdateAsync(string groupName, string updateMessage, string method = "Email");

        Task<string> GetMessageTemplateAsync(string templateName);
        Task UpdateMessageTemplateAsync(string templateName, string templateContent);

        Task<IEnumerable<MessageLog>> GetMessageLogsAsync(string? recipient = null, string? method = null, string? messageType = null, DateTime? startDate = null);
    }
}
