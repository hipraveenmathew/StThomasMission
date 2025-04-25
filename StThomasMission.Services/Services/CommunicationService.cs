using Microsoft.Extensions.Configuration;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

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

            Twilio.TwilioClient.Init(
                _configuration["Twilio:AccountSid"],
                _configuration["Twilio:AuthToken"]
            );

            _templates = new Dictionary<string, string>
            {
                { "AbsenteeNotification", _configuration["Templates:AbsenteeNotification"] ?? "Dear {ParentName}, your child {StudentName} was absent from {Description} on {Date}." },
                { "Announcement", _configuration["Templates:Announcement"] ?? "Dear {Recipient}, {Message}" },
                { "FeeReminder", _configuration["Templates:FeeReminder"] ?? "Dear {ParentName}, this is a reminder to pay your catechism fees for {FeeDetails}." },
                { "GroupUpdate", _configuration["Templates:GroupUpdate"] ?? "Dear {ParentName}, your child's group {GroupName} has an update: {UpdateMessage}" }
            };
        }

        private async Task LogMessageAsync(string recipient, string message, CommunicationChannel method, MessageType messageType)
        {
            if (string.IsNullOrEmpty(recipient)) throw new ArgumentException("Recipient is required.", nameof(recipient));
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("Message is required.", nameof(message));

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
                throw new InvalidOperationException("Twilio SenderId is not configured.");

            await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(senderId),
                to: new PhoneNumber(to)
            );

            await LogMessageAsync(to, message, CommunicationChannel.SMS, MessageType.Notification);
        }

        private async Task SendEmailAsync(string to, string subject, string message)
        {
            if (string.IsNullOrEmpty(to)) return;

            var apiKey = _configuration["SendGrid:ApiKey"];
            var senderEmail = _configuration["SendGrid:SenderEmail"];
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(senderEmail))
                throw new InvalidOperationException("SendGrid configuration is missing.");

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(senderEmail, "St. Thomas Mission");
            var toAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, message, message);
            await client.SendEmailAsync(msg);

            await LogMessageAsync(to, message, CommunicationChannel.Email, MessageType.Notification);
        }

        private async Task SendWhatsAppAsync(string to, string message)
        {
            if (string.IsNullOrEmpty(to)) return;

            var whatsappSender = _configuration["Twilio:WhatsAppSender"];
            if (string.IsNullOrEmpty(whatsappSender))
                throw new InvalidOperationException("WhatsApp sender is not configured.");

            var whatsappTo = $"whatsapp:{to}";
            var whatsappFrom = $"whatsapp:{whatsappSender}";

            await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(whatsappFrom),
                to: new PhoneNumber(whatsappTo)
            );

            await LogMessageAsync(to, message, CommunicationChannel.WhatsApp, MessageType.Notification);
        }

        public async Task SendMessageAsync(string recipient, string message, CommunicationChannel method, MessageType messageType)
        {
            if (string.IsNullOrEmpty(recipient)) throw new ArgumentException("Recipient is required.", nameof(recipient));
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("Message is required.", nameof(message));

            switch (method)
            {
                case CommunicationChannel.SMS:
                    await SendSmsAsync(recipient, message);
                    break;
                case CommunicationChannel.Email:
                    await SendEmailAsync(recipient, "St. Thomas Mission Notification", message);
                    break;
                case CommunicationChannel.WhatsApp:
                    await SendWhatsAppAsync(recipient, message);
                    break;
                default:
                    throw new ArgumentException("Invalid communication method.", nameof(method));
            }
        }

        public async Task SendAnnouncementAsync(string message, string? ward = null, CommunicationChannel method = CommunicationChannel.Email)
        {
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("Message is required.", nameof(message));

            var families = ward == null
                ? await _familyService.GetFamiliesByStatusAsync(FamilyStatus.Active)
                : await _familyService.GetFamiliesByWardAsync(int.Parse(ward)); // Assuming ward is wardId as string

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

                    if (method == CommunicationChannel.SMS && !string.IsNullOrEmpty(parent.Contact))
                        await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.SMS, MessageType.Announcement);
                    else if (method == CommunicationChannel.Email && !string.IsNullOrEmpty(parent.Email))
                        await SendMessageAsync(parent.Email, formattedMessage, CommunicationChannel.Email, MessageType.Announcement);
                    else if (method == CommunicationChannel.WhatsApp && !string.IsNullOrEmpty(parent.Contact))
                        await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.WhatsApp, MessageType.Announcement);
                }
            }
        }

        public async Task SendAbsenteeNotificationAsync(int studentId, DateTime date, CommunicationChannel method = CommunicationChannel.Email)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null)
                throw new ArgumentException("Student not found.", nameof(studentId));

            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
            var family = await _unitOfWork.Families.GetByIdAsync(familyMember.FamilyId);

            var parents = (await _familyService.GetFamilyMembersByFamilyIdAsync(family.Id))
                .Where(m => m.Role == "Parent" && (m.Contact != null || m.Email != null));

            var attendance = (await _unitOfWork.Attendances.GetByStudentIdAsync(studentId))
                .FirstOrDefault(a => a.Date.Date == date.Date && a.Status == AttendanceStatus.Absent);

            if (attendance == null) return;

            var template = await GetMessageTemplateAsync("AbsenteeNotification");

            foreach (var parent in parents)
            {
                var formattedMessage = template
                    .Replace("{ParentName}", $"{parent.FirstName} {parent.LastName}")
                    .Replace("{StudentName}", $"{familyMember.FirstName} {familyMember.LastName}")
                    .Replace("{Description}", attendance.Description)
                    .Replace("{Date}", date.ToString("yyyy-MM-dd"));

                if (method == CommunicationChannel.SMS && !string.IsNullOrEmpty(parent.Contact))
                    await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.SMS, MessageType.Notification);
                else if (method == CommunicationChannel.Email && !string.IsNullOrEmpty(parent.Email))
                    await SendMessageAsync(parent.Email, formattedMessage, CommunicationChannel.Email, MessageType.Notification);
                else if (method == CommunicationChannel.WhatsApp && !string.IsNullOrEmpty(parent.Contact))
                    await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.WhatsApp, MessageType.Notification);
            }
        }

        public async Task SendFeeReminderAsync(int studentId, string feeDetails, CommunicationChannel method = CommunicationChannel.Email)
        {
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

                if (method == CommunicationChannel.SMS && !string.IsNullOrEmpty(parent.Contact))
                    await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.SMS, MessageType.Notification);
                else if (method == CommunicationChannel.Email && !string.IsNullOrEmpty(parent.Email))
                    await SendMessageAsync(parent.Email, formattedMessage, CommunicationChannel.Email, MessageType.Notification);
                else if (method == CommunicationChannel.WhatsApp && !string.IsNullOrEmpty(parent.Contact))
                    await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.WhatsApp, MessageType.Notification);
            }
        }

        public async Task SendGroupUpdateAsync(string groupName, string updateMessage, CommunicationChannel method = CommunicationChannel.Email)
        {
            if (string.IsNullOrEmpty(groupName)) throw new ArgumentException("Group name is required.", nameof(groupName));
            if (string.IsNullOrEmpty(updateMessage)) throw new ArgumentException("Update message is required.", nameof(updateMessage));

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

                    if (method == CommunicationChannel.SMS && !string.IsNullOrEmpty(parent.Contact))
                        await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.SMS, MessageType.Notification);
                    else if (method == CommunicationChannel.Email && !string.IsNullOrEmpty(parent.Email))
                        await SendMessageAsync(parent.Email, formattedMessage, CommunicationChannel.Email, MessageType.Notification);
                    else if (method == CommunicationChannel.WhatsApp && !string.IsNullOrEmpty(parent.Contact))
                        await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.WhatsApp, MessageType.Notification);
                }
            }
        }

        public async Task SendAbsenteeNotificationsAsync(string grade, DateTime? date = null, CommunicationChannel method = CommunicationChannel.Email)
        {
            if (string.IsNullOrEmpty(grade))
                throw new ArgumentException("Grade is required.", nameof(grade));

            var students = await _unitOfWork.Students.GetByGradeAsync(grade);
            var targetDate = date?.Date ?? DateTime.Today;

            foreach (var student in students)
            {
                var attendanceRecords = await _unitOfWork.Attendances.GetByStudentIdAsync(student.Id);
                var targetAttendance = attendanceRecords.FirstOrDefault(a => a.Date.Date == targetDate && a.Status == AttendanceStatus.Absent);

                if (targetAttendance != null)
                    await SendAbsenteeNotificationAsync(student.Id, targetDate, method);
            }
        }

        public async Task<string> GetMessageTemplateAsync(string templateName)
        {
            if (!_templates.TryGetValue(templateName, out var template))
                throw new ArgumentException($"Template '{templateName}' not found.", nameof(templateName));
            return template;
        }

        public async Task UpdateMessageTemplateAsync(string templateName, string templateContent)
        {
            if (string.IsNullOrEmpty(templateName))
                throw new ArgumentException("Template name is required.", nameof(templateName));
            if (string.IsNullOrEmpty(templateContent))
                throw new ArgumentException("Template content is required.", nameof(templateContent));

            _templates[templateName] = templateContent;
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<MessageLog>> GetMessageLogsAsync(string? recipient = null, CommunicationChannel? method = null, MessageType? messageType = null, DateTime? startDate = null)
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