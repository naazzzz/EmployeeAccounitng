using DotNetEnv;
using DotNetEnv.Configuration;
using MassTransit;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Messaging.Consumers;
using NotificationService.Infrastructure.Services;

Env.Load();
    
var builder = Host.CreateApplicationBuilder(args);

IConfigurationRoot configuration = builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath()).Build();

builder.Services.AddScoped<ISmtpClient, SmtpClient>();

builder.Services.AddMassTransit(x =>
{
    var brokerType = configuration.GetValue<string>("BROKER_TYPE") ?? "RabbitMQ";
    var brokerHost = configuration.GetValue<string>("BROKER_HOST") ?? "localhost";

    if (brokerType == "RabbitMQ")
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(brokerHost, h =>
            {
                h.Username("kalo");
                h.Password("kalo");
            });
            
            cfg.ConfigureEndpoints(context);
        });
    }
    x.AddConsumer<NotificationConsumer>();
});

var host = builder.Build();
host.Run();