using MimeKit;

namespace NotificationService.Core.Interfaces;

public interface ISmtpClient
{
    Task SendMessageAsync(InternetAddress toAddress, bool htmlBodyFormat, string message, string subject);
}