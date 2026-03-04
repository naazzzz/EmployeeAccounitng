using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace General.Configurations;

public static class RateLimiterConfiguration
{
    public static void Configure(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.OnRejected = (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                return new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        Error = "Слишком много запросов. Пожалуйста, попробуйте снова через несколько минут."
                    }
                ));
            };
            options.AddFixedWindowLimiter("CreateCodePolicy", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(2);
                opt.PermitLimit = 2;
                opt.QueueLimit = 2;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            options.AddFixedWindowLimiter("FixedConfirmCodePolicy", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(5);
                opt.PermitLimit = 6;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 3;
            });
        });
    }
}