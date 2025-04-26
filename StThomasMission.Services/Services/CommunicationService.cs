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
        private readonly IStudentService _studentService;
        private readonly IGroupService _groupService;
        private readonly IAuditService _auditService;
        private readonly Dictionary<string, string> _templates;

        public CommunicationService(IConfiguration configuration, IUnitOfWork unitOfWork, IFamilyService familyService, IStudentService studentService, IGroupService groupService, IAuditService auditService)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _familyService = familyService;
            _studentService = studentService;
            _groupService = groupService;
            _auditService = auditService;

            Twilio.TwilioClient.Init(
                _configuration["Twilio:AccountSid"],
                _configuration["Twilio:AuthToken"]
            );

            _templates = new Dictionary<string, string>
            {
                { "Announcement", _configuration["Templates:Announcement"] ?? "Dear {Recipient}, {Message}" },
                { "AbsenteeNotification", _configuration["Templates:AbsenteeNotification"] ?? "Dear {ParentName}, your child {StudentName} was absent from {Description} on {Date}." },
                { "FeeReminder", _configuration["Templates:FeeReminder"] ?? "Dear {ParentName}, please pay catechism fees: {FeeDetails}." },
                { "GroupUpdate", _configuration["Templates:GroupUpdate"] ?? "Dear {ParentName}, group {GroupName} update: {UpdateMessage}" }
            };
        }

        public async Task SendMessageAsync(string recipient, string message, CommunicationChannel method, MessageType messageType)
        {
            if (string.IsNullOrEmpty(recipient))
                throw new ArgumentException("Recipient is required.", nameof(recipient));
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Message is required.", nameof(message));

            switch (method)
            {
                case CommunicationChannel.SMS:
                    await SendSmsAsync(recipient, message, messageType);
                    break;
                case CommunicationChannel.Email:
                    await SendEmailAsync(recipient, "St. Thomas Mission Notification", message, messageType);
                    break;
                case CommunicationChannel.WhatsApp:
                    await SendWhatsAppAsync(recipient, message, messageType);
                    break;
                default:
                    throw new ArgumentException("Invalid communication method.", nameof(method));
            }
        }

        public async Task SendAnnouncementAsync(string message, int? wardId = null, CommunicationChannel method = CommunicationChannel.Email)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Message is required.", nameof(message));

            IEnumerable<Family> families = wardId.HasValue
                ? await _unitOfWork.Families.GetByWardAsync(wardId.Value)
                : await _unitOfWork.Families.GetByStatusAsync(FamilyStatus.Active);

            var template = _templates["Announcement"];
            foreach (var family in families)
            {
                var parents = (await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(family.Id))
                    .Where(m => m.Relation == FamilyMemberRole.Parent && (m.Contact != null || m.Email != null));

                foreach (var parent in parents)
                {
                    var formattedMessage = template
                        .Replace("{Recipient}", parent.FullName)
                        .Replace("{Message}", message);

                    if (method == CommunicationChannel.SMS && !string.IsNullOrEmpty(parent.Contact))
                        await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.SMS, MessageType.Announcement);
                    else if (method == CommunicationChannel.Email && !string.IsNullOrEmpty(parent.Email))
                        await SendMessageAsync(parent.Email, formattedMessage, CommunicationChannel.Email, MessageType.Announcement);
                    else if (method == CommunicationChannel.WhatsApp && !string.IsNullOrEmpty(parent.Contact))
                        await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.WhatsApp, MessageType.Announcement);
                }
            }

            await _auditService.LogActionAsync("System", "Send", nameof(MessageLog), "Multiple", $"Sent announcement to {families.Count()} families via {method}");
        }

        public async Task SendAbsenteeNotificationAsync(int studentId, DateTime date, CommunicationChannel method = CommunicationChannel.Email)
        {
            var student = await _studentService.GetStudentByIdAsync(studentId);
            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
            var family = await _unitOfWork.Families.GetByIdAsync(familyMember.FamilyId);

            var attendance = (await _unitOfWork.Attendances.GetByStudentIdAsync(studentId))
                .FirstOrDefault(a => a.Date.Date == date.Date && a.Status == AttendanceStatus.Absent);
            if (attendance == null)
                return;

            var parents = (await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(family.Id))
                .Where(m => m.Relation == FamilyMemberRole.Parent && (m.Contact != null || m.Email != null));

            var template = _templates["AbsenteeNotification"];
            foreach (var parent in parents)
            {
                var formattedMessage = template
                    .Replace("{ParentName}", parent.FullName)
                    .Replace("{StudentName}", familyMember.FullName)
                    .Replace("{Description}", attendance.Description)
                    .Replace("{Date}", date.ToString("yyyy-MM-dd"));

                if (method == CommunicationChannel.SMS && !string.IsNullOrEmpty(parent.Contact))
                    await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.SMS, MessageType.AbsenteeNotification);
                else if (method == CommunicationChannel.Email && !string.IsNullOrEmpty(parent.Email))
                    await SendMessageAsync(parent.Email, formattedMessage, CommunicationChannel.Email, MessageType.AbsenteeNotification);
                else if (method == CommunicationChannel.WhatsApp && !string.IsNullOrEmpty(parent.Contact))
                    await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.WhatsApp, MessageType.AbsenteeNotification);
            }

            await _auditService.LogActionAsync("System", "Send", nameof(MessageLog), studentId.ToString(), $"Sent absentee notification for student {studentId} on {date:yyyy-MM-dd}");
        }

        public async Task SendFeeReminderAsync(int studentId, string feeDetails, CommunicationChannel method = CommunicationChannel.Email)
        {
            if (string.IsNullOrEmpty(feeDetails))
                throw new ArgumentException("Fee details are required.", nameof(feeDetails));

            var student = await _studentService.GetStudentByIdAsync(studentId);
            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
            var family = await _unitOfWork.Families.GetByIdAsync(familyMember.FamilyId);

            var parents = (await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(family.Id))
                .Where(m => m.Relation == FamilyMemberRole.Parent && (m.Contact != null || m.Email != null));

            var template = _templates["FeeReminder"];
            foreach (var parent in parents)
            {
                var formattedMessage = template
                    .Replace("{ParentName}", parent.FullName)
                    .Replace("{FeeDetails}", feeDetails);

                if (method == CommunicationChannel.SMS && !string.IsNullOrEmpty(parent.Contact))
                    await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.SMS, MessageType.FeeReminder);
                else if (method == CommunicationChannel.Email && !string.IsNullOrEmpty(parent.Email))
                    await SendMessageAsync(parent.Email, formattedMessage, CommunicationChannel.Email, MessageType.FeeReminder);
                else if (method == CommunicationChannel.WhatsApp && !string.IsNullOrEmpty(parent.Contact))
                    await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.WhatsApp, MessageType.FeeReminder);
            }

            await _auditService.LogActionAsync("System", "Send", nameof(MessageLog), studentId.ToString(), $"Sent fee reminder for student {studentId}");
        }

        public async Task SendGroupUpdateAsync(int groupId, string updateMessage, CommunicationChannel method = CommunicationChannel.Email)
        {
            if (string.IsNullOrEmpty(updateMessage))
                throw new ArgumentException("Update message is required.", nameof(updateMessage));

            var group = await _groupService.GetGroupByIdAsync(groupId);
            var students = await _unitOfWork.Students.GetByGroupIdAsync(groupId);
            var template = _templates["GroupUpdate"];

            foreach (var student in students)
            {
                var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
                var family = await _unitOfWork.Families.GetByIdAsync(familyMember.FamilyId);
                var parents = (await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(family.Id))
                    .Where(m => m.Relation == FamilyMemberRole.Parent && (m.Contact != null || m.Email != null));

                foreach (var parent in parents)
                {
                    var formattedMessage = template
                        .Replace("{ParentName}", parent.FullName)
                        .Replace("{GroupName}", group.Name)
                        .Replace("{UpdateMessage}", updateMessage);

                    if (method == CommunicationChannel.SMS && !string.IsNullOrEmpty(parent.Contact))
                        await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.SMS, MessageType.GroupUpdate);
                    else if (method == CommunicationChannel.Email && !string.IsNullOrEmpty(parent.Email))
                        await SendMessageAsync(parent.Email, formattedMessage, CommunicationChannel.Email, MessageType.GroupUpdate);
                    else if (method == CommunicationChannel.WhatsApp && !string.IsNullOrEmpty(parent.Contact))
                        await SendMessageAsync(parent.Contact, formattedMessage, CommunicationChannel.WhatsApp, MessageType.GroupUpdate);
                }
            }

            await _auditService.LogActionAsync("System", "Send", nameof(MessageLog), groupId.ToString(), $"Sent group update for group {group.Name}");
        }

        public async Task<IEnumerable<MessageLog>> GetMessageLogsAsync(string? recipient = null, CommunicationChannel? method = null, MessageType? messageType = null, DateTime? startDate = null)
        {
            return await _unitOfWork.MessageLogs.GetAsync(log =>
                (recipient == null || log.Recipient == recipient) &&
                (method == null || log.Method == method) &&
                (messageType == null || log.MessageType == messageType) &&
                (startDate == null || log.SentAt >= startDate));
        }

        private async Task SendSmsAsync(string to, string message, MessageType messageType)
        {
            if (string.IsNullOrEmpty(to))
                return;

            var senderId = _configuration["Twilio:SenderId"];
            if (string.IsNullOrEmpty(senderId))
                throw new InvalidOperationException("Twilio SenderId is not configured.");

            var result = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(senderId),
                to: new PhoneNumber(to)
            );

            var log = new MessageLog
            {
                Recipient = to,
                Message = message,
                Method = CommunicationChannel.SMS,
                MessageType = messageType,
                SentAt = DateTime.UtcNow,
                Status = result.Status.ToString(),
                ResponseDetails = result.Sid,
                SentBy = "System"
            };

            await _unitOfWork.MessageLogs.AddAsync(log);
            await _unitOfWork.CompleteAsync();
        }

        private async Task SendEmailAsync(string to, string subject, string message, MessageType messageType)
        {
            if (string.IsNullOrEmpty(to))
                return;

            var apiKey = _configuration["SendGrid:ApiKey"];
            var senderEmail = _configuration["SendGrid:SenderEmail"];
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(senderEmail))
                throw new InvalidOperationException("SendGrid configuration is missing.");

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(senderEmail, "St. Thomas Mission");
            var toAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, message, message);
            var response = await client.SendEmailAsync(msg);

            var log = new MessageLog
            {
                Recipient = to,
                Message = message,
                Method = CommunicationChannel.Email,
                MessageType = messageType,
                SentAt = DateTime.UtcNow,
                Status = response.StatusCode.ToString(),
                ResponseDetails = await response.Body.ReadAsStringAsync(),
                SentBy = "System"
            };

            await _unitOfWork.MessageLogs.AddAsync(log);
            await _unitOfWork.CompleteAsync();
        }

        private async Task SendWhatsAppAsync(string to, string message, MessageType messageType)
        {
            if (string.IsNullOrEmpty(to))
                return;

            var whatsappSender = _configuration["Twilio:WhatsAppSender"];
            if (string.IsNullOrEmpty(whatsappSender))
                throw new InvalidOperationException("WhatsApp sender is not configured.");

            var whatsappTo = $"whatsapp:{to}";
            var whatsappFrom = $"whatsapp:{whatsappSender}";

            var result = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(whatsappFrom),
                to: new PhoneNumber(whatsappTo)
            );

            var log = new MessageLog
            {
                Recipient = to,
                Message = message,
                Method = CommunicationChannel.WhatsApp,
                MessageType = messageType,
                SentAt = DateTime.UtcNow,
                Status = result.Status.ToString(),
                ResponseDetails = result.Sid,
                SentBy = "System"
            };

            await _unitOfWork.MessageLogs.AddAsync(log);
            await _unitOfWork.CompleteAsync();
        }
    }
}