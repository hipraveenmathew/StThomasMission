namespace StThomasMission.Core.Interfaces
{
    public interface ICommunicationService
    {
        Task SendSmsAsync(string toPhoneNumber, string message);
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendWhatsAppAsync(string toPhoneNumber, string message);
        Task SendAbsenteeNotificationsAsync(int grade);
        Task SendAnnouncementAsync(string message, string? ward = null);
    }
}