using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using StThomasMission.Core.Models.Settings;
using StThomasMission.Services.Interfaces;
using System.Threading.Tasks;

namespace StThomasMission.Services.Messaging
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridSettings _settings;

        public SendGridEmailSender(IOptions<SendGridSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<(string Status, string? Details)> SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var client = new SendGridClient(_settings.ApiKey);
            var from = new EmailAddress(_settings.SenderEmail, _settings.SenderName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlMessage);
            var response = await client.SendEmailAsync(msg);

            string status = response.IsSuccessStatusCode ? "Success" : "Failed";
            string? details = response.IsSuccessStatusCode ? null : await response.Body.ReadAsStringAsync();

            return (status, details);
        }
    }
}