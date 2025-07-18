using System.Threading.Tasks;

namespace StThomasMission.Services.Interfaces
{
    public interface IEmailSender
    {
        Task<(string Status, string? Details)> SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}