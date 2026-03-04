using MailKit.Security;
using MimeKit;
using ISmtpClient = NotificationService.Core.Interfaces.ISmtpClient;

namespace NotificationService.Infrastructure.Services;

public sealed class SmtpClient : ISmtpClient
{
    private readonly IConfiguration _configuration;

    public SmtpClient(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendMessageAsync(InternetAddress toAddress, bool htmlBodyFormat, string message, string subject)
    {
        try
        {
            var fromMail = _configuration.GetValue<string>("MAIL_SMTP_FROM_MAIL");
            var fromPass = _configuration.GetValue<string>("MAIL_SMTP_FROM_PASS");
            var smtpHost = _configuration.GetValue<string>("MAIL_SMTP_HOST");
            var smtpPort = _configuration.GetValue<int>("MAIL_SMTP_PORT");
            Console.WriteLine(fromMail, fromPass, smtpHost, smtpPort);

            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("DBI Employee Accounting", fromMail));
            mailMessage.To.Add(toAddress);
            mailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder();

            if (htmlBodyFormat)
            {
                bodyBuilder.HtmlBody = message;
            }
            else
            {
                bodyBuilder.TextBody = message;
            }

            mailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(fromMail, fromPass);
            await client.SendAsync(mailMessage);
            await client.DisconnectAsync(true);
            Console.WriteLine("Письмо успешно отправлено");
        } catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке письма: {ex.Message}");
        }
    }
    
}