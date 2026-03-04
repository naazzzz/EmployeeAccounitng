using AuthService.Core.Interfaces;

namespace AuthService.Infrastructure.HostedServices;

public sealed class ClearExpireMailCodesTimed: IHostedService, IDisposable
    {
    private readonly ILogger<ClearExpireMailCodesTimed> _logger;
    // безопасный способ для работы с singleton-scop'ами
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer? _timer = null;

    public ClearExpireMailCodesTimed(ILogger<ClearExpireMailCodesTimed> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromMinutes(15));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var mailCodeService = scope.ServiceProvider.GetRequiredService<IСonfirmationCodeService>();
                mailCodeService.RemoveExpiredCodes();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка выполнения NotificationExpirePasswordTimedHostedService: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
