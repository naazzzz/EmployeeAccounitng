using AuthService.Core.Interfaces.Repositories;
using General.Event.NotificationService;
using MassTransit;

namespace AuthService.Infrastructure.HostedServices;

public sealed class NotificationExpirePasswordTimed : IHostedService, IDisposable
{
    private readonly ILogger<NotificationExpirePasswordTimed> _logger;

    // безопасный способ для работы с singleton-scop'ами
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer? _timer;

    public NotificationExpirePasswordTimed(ILogger<NotificationExpirePasswordTimed> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromDays(1));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var users = await userRepository.GetAllUsersWithExpiresPassword();
                if (users.Count == 0) return;

                var queueBus = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                foreach (var user in users)
                    await queueBus.Publish(new PasswordExpiringEvent
                    {
                        UserName = user.UserName!,
                        Email = user.Email!
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка выполнения ClearExpireMailCodesTimedHostedService: {ex.Message}");
        }
    }
}