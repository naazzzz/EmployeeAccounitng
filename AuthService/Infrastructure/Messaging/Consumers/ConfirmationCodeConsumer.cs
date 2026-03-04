using AuthService.Core.Interfaces;
using General.Event.AuthService;
using General.Event.NotificationService;
using MassTransit;

namespace AuthService.Infrastructure.Messaging.Consumers;

public class ConfirmationCodeConsumer : IConsumer<CreateConfirmationCodeEvent>
{
    private readonly ILogger<ConfirmationCodeConsumer> _logger;
    private readonly IСonfirmationCodeService _сonfirmationCodeService;

    public ConfirmationCodeConsumer(ILogger<ConfirmationCodeConsumer> logger,
        IСonfirmationCodeService сonfirmationCodeService)
    {
        _logger = logger;
        _сonfirmationCodeService = сonfirmationCodeService;
    }

    //todo расширить и в будущем вынести всю логику в сервис
    public async Task Consume(ConsumeContext<CreateConfirmationCodeEvent> context)
    {
        try
        {
            var code = await _сonfirmationCodeService.CreateNewCode(context.Message.UserId,
                context.Message.UserChangeHistoryId);

            await context.Publish(new CodeSentEvent()
            {
                Code = code.Code ?? throw new Exception("Code is empty"),
                UserName = code.UserName ?? throw new Exception("UserName is empty"),
                Email = code.Email ?? throw new Exception("Email is empty"),
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка выполнения NotificationExpirePasswordTimedHostedService: {ex.Message}");
        }
    }
}