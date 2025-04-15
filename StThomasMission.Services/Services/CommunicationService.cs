using Microsoft.Extensions.Configuration;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Net.Mail;

namespace StThomasMission.Services
{
    public class CommunicationService : ICommunicationService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFamilyService _familyService;
        private readonly Dictionary<string, string> _templates;

        public CommunicationService(IConfiguration configuration, IUnitOfWork unitOfWork, IFamilyService familyService)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _familyService = familyService;

            // Initialize Twilio client
            Twilio.TwilioClient.Init(
                _configuration["Twilio:AccountSid"],
                _configuration["Twilio:AuthToken"]
            );

            // Load templates from configuration with defaults
            _templates = new Dictionary<string, string>
            {
                { "AbsenteeNotification", _configuration["Templates:AbsenteeNotification"] ?? "Dear {ParentName}, your child {StudentName} was absent from {Description} on {Date}." },
                { "Announcement", _configuration["Templates:Announcement"] ?? "Dear {Recipient}, {Message}" },
                { "FeeReminder", _configuration["Templates:FeeReminder"] ?? "Dear {ParentName}, this is a reminder to pay your catechism fees for {FeeDetails}." },
                { "GroupUpdate", _configuration["Templates:GroupUpdate"] ?? "Dear {ParentName}, your child's group {GroupName} has an update: {UpdateMessage}" }
            };
        }

        private async Task LogMessageAsync(string recipient, string message, string method, string messageType)
        {
            if (string.IsNullOrEmpty(recipient)) throw new ArgumentException("Recipient is required.", nameof(recipient));
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("Message is required.", nameof(message));
            if (string.IsNullOrEmpty(method)) throw new ArgumentException("Method is required.", nameof(method));
            if (string.IsNullOrEmpty(messageType)) throw new ArgumentException("Message type is required.", nameof(messageType));

            var log = new MessageLog
            {
                Recipient = recipient,
                Message = message,
                Method = method,
                MessageType = messageType,
                SentAt = DateTime.UtcNow
            };

            await _unitOfWork.MessageLogs.AddAsync(log);
            await _unitOfWork.CompleteAsync();
        }

        private async Task SendSmsAsync(string to, string message)
        {
            if (string.IsNullOrEmpty(to)) return;

            var senderId = _configuration["Twilio:SenderId"];
            if (string.IsNullOrEmpty(senderId))
            {
                throw new InvalidOperationException("Twilio SenderId is not configured.");
            }

            await Twilio.Rest.Api.V2010.Account.MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(senderId),
                to: new PhoneNumber(to)
            );

            await LogMessageAsync(to, message, "SMS", "Notification");
        }

        private async Task SendEmailAsync(string to, string subject, string message)
        {
            if (string.IsNullOrEmpty(to)) return;

            var apiKey = _configuration["SendGrid:ApiKey"];
            var senderEmail = _configuration["SendGrid:SenderEmail"];
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(senderEmail))
            {
                throw new InvalidOperationException("SendGrid configuration is missing.");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(senderEmail, "St. Thomas Mission");
            var toAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, message, message);
            await client.SendEmailAsync(msg);

            await LogMessageAsync(to, message, "Email", "Notification");
        }

        private async Task SendWhatsAppAsync(string to, string message)
        {
            if (string.IsNullOrEmpty(to)) return;

            var whatsappSender = _configuration["Twilio:WhatsAppSender"];
            if (string.IsNullOrEmpty(whatsappSender))
            {
                throw new InvalidOperationException("WhatsApp sender is not configured.");
            }

            var whatsappTo = $"whatsapp:{to}";
            var whatsappFrom = $"whatsapp:{whatsappSender}";

            await Twilio.Rest.Api.V2010.Account.MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(whatsappFrom),
                to: new PhoneNumber(whatsappTo)
            );

            await LogMessageAsync(to, message, "WhatsApp", "Notification");
        }

        public async Task SendMessageAsync(string recipient, string message, string method, string messageType)
        {
            if (string.IsNullOrEmpty(recipient)) throw new ArgumentException("Recipient is required.", nameof(recipient));
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("Message is required.", nameof(message));
            if (!new[] { "SMS", "Email", "WhatsApp" }.Contains(method)) throw new ArgumentException("Invalid method.", nameof(method));
            if (string.IsNullOrEmpty(messageType)) throw new ArgumentException("Message type is required.", nameof(messageType));

            if (method == "SMS")
            {
                await SendSmsAsync(recipient, message);
            }
            else if (method == "Email")
            {
                await SendEmailAsync(recipient, "St. Thomas Mission Notification", message);
            }
            else if (method == "WhatsApp")
            {
                await SendWhatsAppAsync(recipient, message);
            }
        }

        public async Task SendAnnouncementAsync(string message, string? ward = null, string method = "Email")
        {
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("Message is required.", nameof(message));
            if (!new[] { "SMS", "Email", "WhatsApp" }.Contains(method)) throw new ArgumentException("Invalid method.", nameof(method));

            var families = ward == null
                ? await _familyService.GetFamiliesByStatusAsync("Active")
                : await _familyService.GetFamiliesByWardAsync(ward);

            var template = await GetMessageTemplateAsync("Announcement");
            foreach (var family in families)
            {
                var parents = (await _familyService.GetFamilyMembersByFamilyIdAsync(family.Id))
                    .Where(m => m.Role == "Parent" && (m.Contact != null || m.Email != null));
                foreach (var parent in parents)
                {
                    var formattedMessage = template
                        .Replace("{Recipient}", $"{parent.FirstName} {parent.LastName}")
                        .Replace("{Message}", message);

                    if (method == "SMS" && !string.IsNullOrEmpty(parent.Contact))
                    {
                        await SendMessageAsync(parent.Contact, formattedMessage, "SMS", "Announcement");
                    }
                    else if (method == "Email" && !string.IsNullOrEmpty(parent.Email))
                    {
                        await SendMessageAsync(parent.Email, formattedMessage, "Email", "Announcement");
                    }
                    else if (method == "WhatsApp" && !string.IsNullOrEmpty(parent.Contact))
                    {
                        await SendMessageAsync(parent.Contact, formattedMessage, "WhatsApp", "Announcement");
                    }
                }
            }
        }

        public async Task SendAbsenteeNotificationAsync(int studentId, DateTime date, string method = "Email")
        {
            if (!new[] { "SMS", "Email", "WhatsApp" }.Contains(method)) throw new ArgumentException("Invalid method.", nameof(method));

            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) throw new ArgumentException("Student not found.", nameof(studentId));

            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
            var family = await _unitOfWork.Families.GetByIdAsync(familyMember.FamilyId);
            var parents = (await _familyService.GetFamilyMembersByFamilyIdAsync(family.Id))
                .Where(m => m.Role == "Parent" && (m.Contact != null || m.Email != null));

            var attendance = (await _unitOfWork.Attendances.GetByStudentIdAsync(studentId))
                .FirstOrDefault(a => a.Date.Date == date.Date && !a.IsPresent);
            if (attendance == null) return; // No absence to notify

            var template = await GetMessageTemplateAsync("AbsenteeNotification");
            foreach (var parent in parents)
            {
                var formattedMessage = template
                    .Replace("{ParentName}", $"{parent.FirstName} {parent.LastName}")
                    .Replace("{StudentName}", $"{familyMember.FirstName} {familyMember.LastName}")
                    .Replace("{Description}", attendance.Description)
                    .Replace("{Date}", date.ToString("yyyy-MM-dd"));

                if (method == "SMS" && !string.IsNullOrEmpty(parent.Contact))
                {
                    await SendMessageAsync(parent.Contact, formattedMessage, "SMS", "Notification");
                }
                else if (method == "Email" && !string.IsNullOrEmpty(parent.Email))
                {
                    await SendMessageAsync(parent.Email, formattedMessage, "Email", "Notification");
                }
                else if (method == "WhatsApp" && !string.IsNullOrEmpty(parent.Contact))
                {
                    await SendMessageAsync(parent.Contact, formattedMessage, "WhatsApp", "Notification");
                }
            }
        }

        public async Task SendFeeReminderAsync(int studentId, string feeDetails, string method = "Email")
        {
            if (!new[] { "SMS", "Email", "WhatsApp" }.Contains(method)) throw new ArgumentException("Invalid method.", nameof(method));

            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) throw new ArgumentException("Student not found.", nameof(studentId));

            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
            var family = await _unitOfWork.Families.GetByIdAsync(familyMember.FamilyId);
            var parents = (await _familyService.GetFamilyMembersByFamilyIdAsync(family.Id))
                .Where(m => m.Role == "Parent" && (m.Contact != null || m.Email != null));

            var template = await GetMessageTemplateAsync("FeeReminder");
            foreach (var parent in parents)
            {
                var formattedMessage = template
                    .Replace("{ParentName}", $"{parent.FirstName} {parent.LastName}")
                    .Replace("{FeeDetails}", feeDetails);

                if (method == "SMS" && !string.IsNullOrEmpty(parent.Contact))
                {
                    await SendMessageAsync(parent.Contact, formattedMessage, "SMS", "Notification");
                }
                else if (method == "Email" && !string.IsNullOrEmpty(parent.Email))
                {
                    await SendMessageAsync(parent.Email, formattedMessage, "Email", "Notification");
                }
                else if (method == "WhatsApp" && !string.IsNullOrEmpty(parent.Contact))
                {
                    await SendMessageAsync(parent.Contact, formattedMessage, "WhatsApp", "Notification");
                }
            }
        }

        public async Task SendGroupUpdateAsync(string groupName, string updateMessage, string method = "Email")
        {
            if (string.IsNullOrEmpty(groupName)) throw new ArgumentException("Group name is required.", nameof(groupName));
            if (string.IsNullOrEmpty(updateMessage)) throw new ArgumentException("Update message is required.", nameof(updateMessage));
            if (!new[] { "SMS", "Email", "WhatsApp" }.Contains(method)) throw new ArgumentException("Invalid method.", nameof(method));

            var students = await _unitOfWork.Students.GetByGroupAsync(groupName);
            var template = await GetMessageTemplateAsync("GroupUpdate");

            foreach (var student in students)
            {
                var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
                var family = await _unitOfWork.Families.GetByIdAsync(familyMember.FamilyId);
                var parents = (await _familyService.GetFamilyMembersByFamilyIdAsync(family.Id))
                    .Where(m => m.Role == "Parent" && (m.Contact != null || m.Email != null));

                foreach (var parent in parents)
                {
                    var formattedMessage = template
                        .Replace("{ParentName}", $"{parent.FirstName} {parent.LastName}")
                        .Replace("{GroupName}", groupName)
                        .Replace("{UpdateMessage}", updateMessage);

                    if (method == "SMS" && !string.IsNullOrEmpty(parent.Contact))
                    {
                        await SendMessageAsync(parent.Contact, formattedMessage, "SMS", "Notification");
                    }
                    else if (method == "Email" && !string.IsNullOrEmpty(parent.Email))
                    {
                        await SendMessageAsync(parent.Email, formattedMessage, "Email", "Notification");
                    }
                    else if (method == "WhatsApp" && !string.IsNullOrEmpty(parent.Contact))
                    {
                        await SendMessageAsync(parent.Contact, formattedMessage, "WhatsApp", "Notification");
                    }
                }
            }
        }

        public async Task<string> GetMessageTemplateAsync(string templateName)
        {
            if (!_templates.TryGetValue(templateName, out var template))
            {
                throw new ArgumentException($"Template '{templateName}' not found.", nameof(templateName));
            }
            return await Task.FromResult(template);
        }

        public async Task UpdateMessageTemplateAsync(string templateName, string templateContent)
        {
            if (string.IsNullOrEmpty(templateName)) throw new ArgumentException("Template name is required.", nameof(templateName));
            if (string.IsNullOrEmpty(templateContent)) throw new ArgumentException("Template content is required.", nameof(templateContent));

            _templates[templateName] = templateContent;
            await Task.CompletedTask; // Simulate async operation
        }

        public async Task<IEnumerable<MessageLog>> GetMessageLogsAsync(string? recipient = null, string? method = null, string? messageType = null, DateTime? startDate = null)
        {
            var logs = await _unitOfWork.MessageLogs.GetAllAsync();
            return logs.Where(log =>
                (recipient == null || log.Recipient == recipient) &&
                (method == null || log.Method == method) &&
                (messageType == null || log.MessageType == messageType) &&
                (startDate == null || log.SentAt >= startDate));
        }
    }
}