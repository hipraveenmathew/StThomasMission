using Microsoft.Extensions.Configuration;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System.Net.Mail;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML.Messaging;
using Twilio.Types;

namespace StThomasMission.Services.Services
{
    public class CommunicationService : ICommunicationService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Dictionary<string, string> _templates;

        public CommunicationService(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;

            Twilio.TwilioClient.Init(
                _configuration["Twilio:AccountSid"],
                _configuration["Twilio:AuthToken"]
            );

            // Load templates from configuration
            _templates = new Dictionary<string, string>
    {
        { "AbsenteeNotification", _configuration["Templates:AbsenteeNotification"] ?? "Dear {0}, we missed you in catechism class today. Hope to see you next time!" },
        { "Announcement", _configuration["Templates:Announcement"] ?? "{0}" },
        { "FeeReminder", _configuration["Templates:FeeReminder"] ?? "Dear {0}, this is a reminder to pay your catechism fees for {1}." },
        { "GroupUpdate", _configuration["Templates:GroupUpdate"] ?? "Dear {0}, your group {1} has an update: {2}" }
    };
        }

        private async Task LogMessageAsync(string recipient, string message, string method)
        {
            var log = new MessageLog
            {
                Recipient = recipient,
                Message = message,
                Method = method,
                SentAt = DateTime.UtcNow
            };
            _unitOfWork._context.MessageLogs.Add(log);
            await _unitOfWork.CompleteAsync();
        }

        // Update SendSmsAsync, SendEmailAsync, SendWhatsAppAsync to log messages
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
                from: new Twilio.Types.PhoneNumber(senderId),
                to: new Twilio.Types.PhoneNumber(to)
            );

            await LogMessageAsync(to, message, "SMS");
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

            await LogMessageAsync(to, message, "Email");
        }
        public async Task SendAnnouncementAsync(string message, string? ward = null, List<string> communicationMethods = null)
        {
            communicationMethods ??= new List<string> { "SMS" }; // Default to SMS if not specified

            var families = await _unitOfWork.Families.GetAllAsync();
            if (!string.IsNullOrEmpty(ward))
            {
                families = families.Where(f => f.Ward == ward).ToList();
            }

            string messageTemplate = _templates["Announcement"];
            string formattedMessage = string.Format(messageTemplate, message);

            foreach (var family in families)
            {
                var members = family.Members.Where(m => !string.IsNullOrEmpty(m.Contact) || !string.IsNullOrEmpty(m.Email)).ToList();
                foreach (var member in members)
                {
                    foreach (var method in communicationMethods)
                    {
                        if (method == "SMS" && !string.IsNullOrEmpty(member.Contact))
                        {
                            await SendSmsAsync(member.Contact, formattedMessage);
                        }
                        if (method == "Email" && !string.IsNullOrEmpty(member.Email))
                        {
                            await SendEmailAsync(member.Email, "Parish Announcement", formattedMessage);
                        }
                        if (method == "WhatsApp" && !string.IsNullOrEmpty(member.Contact))
                        {
                            await SendWhatsAppAsync(member.Contact, formattedMessage);
                        }
                    }
                }
            }
        }

        // Update other methods similarly (SendAbsenteeNotificationsAsync, SendFeeReminderAsync, SendGroupUpdateAsync)
        public async Task SendAbsenteeNotificationsAsync(int grade, List<string> communicationMethods = null)
        {
            communicationMethods ??= new List<string> { "SMS" };

            var students = await _unitOfWork.Students.GetByGradeAsync($"Year {grade}");
            var today = DateTime.Today;
            var attendanceRecords = await _unitOfWork.Attendances.GetByDateAsync(today);

            var absentees = students
                .Where(s => !attendanceRecords.Any(a => a.StudentId == s.Id && a.IsPresent))
                .ToList();

            if (!absentees.Any()) return;

            string messageTemplate = _templates["AbsenteeNotification"];
            foreach (var student in absentees)
            {
                var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
                if (familyMember == null) continue;

                string message = string.Format(messageTemplate, familyMember.FirstName);
                foreach (var method in communicationMethods)
                {
                    if (method == "SMS" && !string.IsNullOrEmpty(familyMember.Contact))
                    {
                        await SendSmsAsync(familyMember.Contact, message);
                    }
                    if (method == "Email" && !string.IsNullOrEmpty(familyMember.Email))
                    {
                        await SendEmailAsync(familyMember.Email, "Absentee Notification", message);
                    }
                    if (method == "WhatsApp" && !string.IsNullOrEmpty(familyMember.Contact))
                    {
                        await SendWhatsAppAsync(familyMember.Contact, message);
                    }
                }
            }
        }

        public async Task SendFeeReminderAsync(int studentId, string feeDetails, List<string> communicationMethods = null)
        {
            communicationMethods ??= new List<string> { "SMS" };

            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) return;

            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
            if (familyMember == null) return;

            string messageTemplate = _templates["FeeReminder"];
            string message = string.Format(messageTemplate, familyMember.FirstName, feeDetails);

            foreach (var method in communicationMethods)
            {
                if (method == "SMS" && !string.IsNullOrEmpty(familyMember.Contact))
                {
                    await SendSmsAsync(familyMember.Contact, message);
                }
                if (method == "Email" && !string.IsNullOrEmpty(familyMember.Email))
                {
                    await SendEmailAsync(familyMember.Email, "Fee Reminder", message);
                }
                if (method == "WhatsApp" && !string.IsNullOrEmpty(familyMember.Contact))
                {
                    await SendWhatsAppAsync(familyMember.Contact, message);
                }
            }
        }

        public async Task SendGroupUpdateAsync(string groupName, string updateMessage, List<string> communicationMethods = null)
        {
            communicationMethods ??= new List<string> { "SMS" };

            var students = await _unitOfWork.Students.GetAllAsync();
            var groupStudents = students.Where(s => s.Group == groupName).ToList();

            string messageTemplate = _templates["GroupUpdate"];
            foreach (var student in groupStudents)
            {
                var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
                if (familyMember == null) continue;

                string message = string.Format(messageTemplate, familyMember.FirstName, groupName, updateMessage);
                foreach (var method in communicationMethods)
                {
                    if (method == "SMS" && !string.IsNullOrEmpty(familyMember.Contact))
                    {
                        await SendSmsAsync(familyMember.Contact, message);
                    }
                    if (method == "Email" && !string.IsNullOrEmpty(familyMember.Email))
                    {
                        await SendEmailAsync(familyMember.Email, "Group Update", message);
                    }
                    if (method == "WhatsApp" && !string.IsNullOrEmpty(familyMember.Contact))
                    {
                        await SendWhatsAppAsync(familyMember.Contact, message);
                    }
                }
            }
        }

        public async Task SendAbsenteeNotificationsAsync(int grade)
        {
            var students = await _unitOfWork.Students.GetByGradeAsync($"Year {grade}");
            var today = DateTime.Today;
            var attendanceRecords = await _unitOfWork.Attendances.GetByDateAsync(today);

            var absentees = students
                .Where(s => !attendanceRecords.Any(a => a.StudentId == s.Id && a.IsPresent))
                .ToList();

            if (!absentees.Any()) return;

            string messageTemplate = _templates["AbsenteeNotification"];
            foreach (var student in absentees)
            {
                var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
                if (!string.IsNullOrEmpty(familyMember?.Contact))
                {
                    string message = string.Format(messageTemplate, familyMember.FirstName);
                    await SendSmsAsync(familyMember.Contact, message);
                }
                if (!string.IsNullOrEmpty(familyMember?.Email))
                {
                    await SendEmailAsync(familyMember.Email, "Absentee Notification", message);
                }
            }
        }

        public async Task SendFeeReminderAsync(int studentId, string feeDetails)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) return;

            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
            if (familyMember == null) return;

            string messageTemplate = _templates["FeeReminder"];
            string message = string.Format(messageTemplate, familyMember.FirstName, feeDetails);

            if (!string.IsNullOrEmpty(familyMember.Contact))
            {
                await SendSmsAsync(familyMember.Contact, message);
            }
            if (!string.IsNullOrEmpty(familyMember.Email))
            {
                await SendEmailAsync(familyMember.Email, "Fee Reminder", message);
            }
        }

        public async Task SendGroupUpdateAsync(string groupName, string updateMessage)
        {
            var students = await _unitOfWork.Students.GetAllAsync();
            var groupStudents = students.Where(s => s.Group == groupName).ToList();

            string messageTemplate = _templates["GroupUpdate"];
            foreach (var student in groupStudents)
            {
                var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
                if (familyMember == null) continue;

                string message = string.Format(messageTemplate, familyMember.FirstName, groupName, updateMessage);
                if (!string.IsNullOrEmpty(familyMember.Contact))
                {
                    await SendSmsAsync(familyMember.Contact, message);
                }
                if (!string.IsNullOrEmpty(familyMember.Email))
                {
                    await SendEmailAsync(familyMember.Email, "Group Update", message);
                }
            }
        }

        public async Task SendAnnouncementAsync(string message, string? ward = null)
        {
            var families = await _unitOfWork.Families.GetAllAsync();
            if (!string.IsNullOrEmpty(ward))
            {
                families = families.Where(f => f.Ward == ward).ToList();
            }

            string messageTemplate = _templates["Announcement"];
            string formattedMessage = string.Format(messageTemplate, message);

            foreach (var family in families)
            {
                var members = family.Members.Where(m => !string.IsNullOrEmpty(m.Contact) || !string.IsNullOrEmpty(m.Email)).ToList();
                foreach (var member in members)
                {
                    if (!string.IsNullOrEmpty(member.Contact))
                    {
                        await SendSmsAsync(member.Contact, formattedMessage);
                    }
                    if (!string.IsNullOrEmpty(member.Email))
                    {
                        await SendEmailAsync(member.Email, "Parish Announcement", formattedMessage);
                    }
                }
            }
        }

        public async Task SendSmsAsync(string toPhoneNumber, string message)
        {
            await Task.Run(() =>
            {
                MessageResource.Create(
                    from: new PhoneNumber(_configuration["Twilio:SenderId"]),
                    to: new PhoneNumber(toPhoneNumber),
                    body: message
                );
            });
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.sendgrid.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential(
                    "apikey",
                    _configuration["SendGrid:ApiKey"]
                ),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["SendGrid:SenderEmail"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
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
                from: new Twilio.Types.PhoneNumber(whatsappFrom),
                to: new Twilio.Types.PhoneNumber(whatsappTo)
            );

            // Log the message
            await LogMessageAsync(to, message, "WhatsApp");
        }

        //public async Task SendAbsenteeNotificationsAsync(int grade)
        //{
        //    var students = await _unitOfWork.Students.GetByGradeAsync($"Year {grade}");
        //    var today = DateTime.Today;
        //    var attendanceRecords = await _unitOfWork.Attendances.GetByDateAsync(today);

        //    var absentees = students
        //        .Where(s => !attendanceRecords.Any(a => a.StudentId == s.Id && a.IsPresent))
        //        .ToList();

        //    if (!absentees.Any()) return;

        //    string messageTemplate = "Dear {0}, we missed you in catechism class today. Hope to see you next time!";
        //    foreach (var student in absentees)
        //    {
        //        var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
        //        if (!string.IsNullOrEmpty(familyMember?.Contact))
        //        {
        //            string message = string.Format(messageTemplate, familyMember.FirstName);
        //            await SendSmsAsync(familyMember.Contact, message);
        //        }
        //        if (!string.IsNullOrEmpty(familyMember?.Email))
        //        {
        //            await SendEmailAsync(familyMember.Email, "Absentee Notification", messageTemplate.Replace("{0}", familyMember.FirstName));
        //        }
        //    }
        //}

        //public async Task SendAnnouncementAsync(string message, string? ward = null)
        //{
        //    var families = await _unitOfWork.Families.GetAllAsync();
        //    if (!string.IsNullOrEmpty(ward))
        //    {
        //        families = families.Where(f => f.Ward == ward).ToList();
        //    }

        //    foreach (var family in families)
        //    {
        //        var members = family.Members.Where(m => !string.IsNullOrEmpty(m.Contact) || !string.IsNullOrEmpty(m.Email)).ToList();
        //        foreach (var member in members)
        //        {
        //            if (!string.IsNullOrEmpty(member.Contact))
        //            {
        //                await SendSmsAsync(member.Contact, message);
        //            }
        //            if (!string.IsNullOrEmpty(member.Email))
        //            {
        //                await SendEmailAsync(member.Email, "Parish Announcement", message);
        //            }
        //        }
        //    }
        //}
    }
}