using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace StThomasMission.Services
{
    public class CommunicationService : ICommunicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;

        private readonly string _twilioAccountSid;
        private readonly string _twilioAuthToken;
        private readonly string _twilioSenderId;
        private readonly string _twilioWhatsAppSender;
        private readonly string _sendGridApiKey;
        private readonly string _sendGridSenderEmail;

        public CommunicationService(IUnitOfWork unitOfWork, IConfiguration configuration, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _auditService = auditService;

            _twilioAccountSid = _configuration["Twilio:AccountSid"];
            _twilioAuthToken = _configuration["Twilio:AuthToken"];
            _twilioSenderId = _configuration["Twilio:SenderId"];
            _twilioWhatsAppSender = _configuration["Twilio:WhatsAppSender"];
            _sendGridApiKey = _configuration["SendGrid:ApiKey"];
            _sendGridSenderEmail = _configuration["SendGrid:SenderEmail"];
        }

        public async Task SendAnnouncementAsync(string message, int wardId, string method)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Message is required.", nameof(message));
            if (wardId <= 0)
                throw new ArgumentException("Ward ID must be a positive integer.", nameof(wardId));
            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("Communication method is required.", nameof(method));

            var recipients = await _unitOfWork.FamilyMembers.GetAsync(fm => fm.Family.WardId == wardId);
            foreach (var recipient in recipients)
            {
                if (string.IsNullOrEmpty(recipient.Contact) && string.IsNullOrEmpty(recipient.Email))
                    continue;

                string formattedMessage = _configuration[$"Templates:Announcement"]
                    .Replace("{Recipient}", recipient.FirstName)
                    .Replace("{Message}", message);

                switch (method.ToLower())
                {
                    case "sms":
                        if (!string.IsNullOrEmpty(recipient.Contact))
                            await SendSmsAsync(recipient.Contact, formattedMessage);
                        break;
                    case "email":
                        if (!string.IsNullOrEmpty(recipient.Email))
                            await SendEmailAsync(recipient.Email, "Announcement", formattedMessage);
                        break;
                    case "whatsapp":
                        if (!string.IsNullOrEmpty(recipient.Contact))
                            await SendWhatsAppAsync(recipient.Contact, formattedMessage);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported communication method: {method}", nameof(method));
                }
            }

            await _auditService.LogActionAsync("System", "Send", nameof(MessageLog), wardId.ToString(), $"Sent announcement to ward {wardId} via {method}");
        }
        public async Task SendGroupUpdateAsync(int groupId, string updateMessage, string method)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId);
            if (group == null)
                throw new ArgumentException("Group not found.", nameof(groupId));

            var students = await _unitOfWork.Students.GetByGroupIdAsync(groupId);
            foreach (var student in students)
            {
                var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
                if (familyMember == null)
                    continue;

                var parent = (await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(familyMember.FamilyId))
                    .FirstOrDefault(fm => fm.Relation == Core.Enums.FamilyMemberRole.Parent);
                if (parent == null || (string.IsNullOrEmpty(parent.Contact) && string.IsNullOrEmpty(parent.Email)))
                    continue;

                string message = _configuration[$"Templates:GroupUpdate"]
                    .Replace("{ParentName}", parent.FirstName)
                    .Replace("{GroupName}", group.Name)
                    .Replace("{UpdateMessage}", updateMessage);

                switch (method.ToLower())
                {
                    case "sms":
                        if (!string.IsNullOrEmpty(parent.Contact))
                            await SendSmsAsync(parent.Contact, message);
                        break;
                    case "email":
                        if (!string.IsNullOrEmpty(parent.Email))
                            await SendEmailAsync(parent.Email, "Group Update", message);
                        break;
                    case "whatsapp":
                        if (!string.IsNullOrEmpty(parent.Contact))
                            await SendWhatsAppAsync(parent.Contact, message);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported communication method: {method}", nameof(method));
                }
            }

            await _auditService.LogActionAsync("System", "Send", nameof(MessageLog), groupId.ToString(), $"Sent group update to group {groupId}");
        }
        public async Task SendAbsenteeNotificationsAsync(string grade, string method)
        {
            if (string.IsNullOrEmpty(grade))
                throw new ArgumentException("Grade is required.", nameof(grade));
            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("Communication method is required.", nameof(method));

            var students = await _unitOfWork.Students.GetByGradeAsync(grade);
            var date = DateTime.Today;

            foreach (var student in students)
            {
                var attendance = (await _unitOfWork.Attendances.GetByStudentIdAsync(student.Id))
                    .FirstOrDefault(a => a.Date.Date == date.Date);

                if (attendance != null && attendance.Status == Core.Enums.AttendanceStatus.Absent)
                {
                    var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
                    if (familyMember == null)
                        continue;

                    var parent = (await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(familyMember.FamilyId))
                        .FirstOrDefault(fm => fm.Relation == Core.Enums.FamilyMemberRole.Parent);
                    if (parent == null || (string.IsNullOrEmpty(parent.Contact) && string.IsNullOrEmpty(parent.Email)))
                        continue;

                    string message = _configuration[$"Templates:AbsenteeNotification"]
                        .Replace("{ParentName}", parent.FirstName)
                        .Replace("{StudentName}", familyMember.FirstName)
                        .Replace("{Description}", attendance.Description ?? "Absent from catechism class")
                        .Replace("{Date}", date.ToString("yyyy-MM-dd"));

                    switch (method.ToLower())
                    {
                        case "sms":
                            if (!string.IsNullOrEmpty(parent.Contact))
                                await SendSmsAsync(parent.Contact, message);
                            break;
                        case "email":
                            if (!string.IsNullOrEmpty(parent.Email))
                                await SendEmailAsync(parent.Email, "Absentee Notification", message);
                            break;
                        case "whatsapp":
                            if (!string.IsNullOrEmpty(parent.Contact))
                                await SendWhatsAppAsync(parent.Contact, message);
                            break;
                        default:
                            throw new ArgumentException($"Unsupported communication method: {method}", nameof(method));
                    }

                    await _auditService.LogActionAsync("System", "Send", nameof(MessageLog), student.Id.ToString(), $"Sent absentee notification for student {student.Id}");
                }
            }
        }

        public async Task SendFeeReminderAsync(int familyId, string feeDetails)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null)
                throw new ArgumentException("Family not found.", nameof(familyId));

            var parent = (await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(familyId))
                .FirstOrDefault(fm => fm.Relation == FamilyMemberRole.Parent);
            if (parent == null || (string.IsNullOrEmpty(parent.Contact) && string.IsNullOrEmpty(parent.Email)))
                throw new InvalidOperationException("No parent with contact information found for this family.");

            string message = _configuration[$"Templates:FeeReminder"]
                .Replace("{ParentName}", parent.FirstName)
                .Replace("{FeeDetails}", feeDetails);

            if (!string.IsNullOrEmpty(parent.Contact))
                await SendSmsAsync(parent.Contact, message);
            if (!string.IsNullOrEmpty(parent.Email))
                await SendEmailAsync(parent.Email, "Fee Reminder", message);

            await _auditService.LogActionAsync("System", "Send", nameof(MessageLog), familyId.ToString(), $"Sent fee reminder to family {familyId}");
        }

        public async Task SendGroupUpdateAsync(int groupId, string updateMessage)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId);
            if (group == null)
                throw new ArgumentException("Group not found.", nameof(groupId));

            var students = await _unitOfWork.Students.GetByGroupIdAsync(groupId);
            foreach (var student in students)
            {
                var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
                if (familyMember == null)
                    continue;

                var parent = (await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(familyMember.FamilyId))
                    .FirstOrDefault(fm => fm.Relation == FamilyMemberRole.Parent);
                if (parent == null || (string.IsNullOrEmpty(parent.Contact) && string.IsNullOrEmpty(parent.Email)))
                    continue;

                string message = _configuration[$"Templates:GroupUpdate"]
                    .Replace("{ParentName}", parent.FirstName)
                    .Replace("{GroupName}", group.Name)
                    .Replace("{UpdateMessage}", updateMessage);

                if (!string.IsNullOrEmpty(parent.Contact))
                    await SendSmsAsync(parent.Contact, message);
                if (!string.IsNullOrEmpty(parent.Email))
                    await SendEmailAsync(parent.Email, "Group Update", message);
            }

            await _auditService.LogActionAsync("System", "Send", nameof(MessageLog), groupId.ToString(), $"Sent group update to group {groupId}");
        }

        public IQueryable<MessageLog> GetMessageHistoryQueryable(string searchString = null)
        {
            var query = _unitOfWork.MessageLogs.GetQueryable(m => true);

            if (!string.IsNullOrEmpty(searchString))
            {
                var searchLower = searchString.ToLower();
                query = query.Where(m => m.Recipient.ToLower().Contains(searchLower) ||
                                         m.Message.ToLower().Contains(searchLower));
            }

            return query;
        }

        private async Task SendSmsAsync(string phoneNumber, string message)
        {
            TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);
            var smsMessage = await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(_twilioSenderId),
                to: new Twilio.Types.PhoneNumber(phoneNumber)
            );

            await LogMessageAsync(phoneNumber, message, "SMS", smsMessage.Status.ToString(), smsMessage.ErrorMessage);
        }

        private async Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress(_sendGridSenderEmail, "St. Thomas Mission");
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, message, message);
            var response = await client.SendEmailAsync(msg);

            await LogMessageAsync(email, message, "Email", response.StatusCode.ToString(), await response.Body.ReadAsStringAsync());
        }

        private async Task SendWhatsAppAsync(string phoneNumber, string message)
        {
            TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);
            var whatsAppMessage = await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber($"whatsapp:{_twilioWhatsAppSender}"),
                to: new Twilio.Types.PhoneNumber($"whatsapp:{phoneNumber}")
            );

            await LogMessageAsync(phoneNumber, message, "WhatsApp", whatsAppMessage.Status.ToString(), whatsAppMessage.ErrorMessage);
        }

        private async Task LogMessageAsync(string recipient, string message, string method, string status, string responseDetails)
        {
            var messageLog = new MessageLog
            {
                Recipient = recipient,
                Message = message,
                Method = Enum.Parse<CommunicationChannel>(method, true), // Convert string to enum
                Status = status,
                ResponseDetails = responseDetails,
                SentBy = "System",
                SentAt = DateTime.UtcNow
            };

            await _unitOfWork.MessageLogs.AddAsync(messageLog);
            await _unitOfWork.CompleteAsync();
        }
    }
}