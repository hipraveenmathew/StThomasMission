using Microsoft.Extensions.Configuration;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System.Net.Mail;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace StThomasMission.Services.Services
{
    public class CommunicationService : ICommunicationService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public CommunicationService(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;

            // Initialize Twilio
            Twilio.TwilioClient.Init(
                _configuration["Twilio:AccountSid"],
                _configuration["Twilio:AuthToken"]
            );
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

        public async Task SendWhatsAppAsync(string toPhoneNumber, string message)
        {
            await Task.Run(() =>
            {
                MessageResource.Create(
                    from: new PhoneNumber($"whatsapp:{_configuration["Twilio:WhatsAppSender"]}"),
                    to: new PhoneNumber($"whatsapp:{toPhoneNumber}"),
                    body: message
                );
            });
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

            string messageTemplate = "Dear {0}, we missed you in catechism class today. Hope to see you next time!";
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
                    await SendEmailAsync(familyMember.Email, "Absentee Notification", messageTemplate.Replace("{0}", familyMember.FirstName));
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

            foreach (var family in families)
            {
                var members = family.Members.Where(m => !string.IsNullOrEmpty(m.Contact) || !string.IsNullOrEmpty(m.Email)).ToList();
                foreach (var member in members)
                {
                    if (!string.IsNullOrEmpty(member.Contact))
                    {
                        await SendSmsAsync(member.Contact, message);
                    }
                    if (!string.IsNullOrEmpty(member.Email))
                    {
                        await SendEmailAsync(member.Email, "Parish Announcement", message);
                    }
                }
            }
        }
    }
}