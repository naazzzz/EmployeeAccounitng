using General.Event.NotificationService;
using MassTransit;
using MimeKit;
using NotificationService.Core.Interfaces;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class NotificationConsumer: IConsumer<UserTransferredEvent>, IConsumer<CodeSentEvent>, IConsumer<PasswordExpiringEvent>
{
    private readonly ISmtpClient _smtpClient;

    public NotificationConsumer(ISmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
    }

    //todo расширить и в будущем вынести всю логику в сервис
    public async Task Consume(ConsumeContext<UserTransferredEvent> context)
    {
         await _smtpClient.SendMessageAsync(
            new MailboxAddress(context.Message.UserName, context.Message.Email),
            false,
            $"Вы были переведены в новый департамент - {context.Message.DepartmentName}",
            "Notification DBI Message");
    }

    public async Task Consume(ConsumeContext<CodeSentEvent> context)
    {
        await _smtpClient.SendMessageAsync(
            new MailboxAddress(context.Message.UserName, context.Message.Email),
            false,
            context.Message.Code,
        "2FA DBI Message");
    }

    public async Task Consume(ConsumeContext<PasswordExpiringEvent> context)
    {
        await _smtpClient.SendMessageAsync(
            new MailboxAddress(context.Message.UserName, context.Message.Email),
            false,
            "Истекает срок действия вашего пароля.",
            "Notification DBI Message");
    }
}