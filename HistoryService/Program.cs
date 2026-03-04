using System.Text.Json.Serialization;
using DotNetEnv;
using DotNetEnv.Configuration;
using General.Configurations;
using HistoryService.Core.Mappings.ChangeHistory;
using HistoryService.Infrastructure;
using HistoryService.Infrastructure.Messaging.Consumers;
using HistoryService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using LoadOptions = DotNetEnv.LoadOptions;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

IConfigurationRoot configuration = builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath()).Build();

builder.Services.AddDbContext<ApplicationContext>((serviceProvider, options) =>
    options.UseNpgsql(configuration.GetValue<string>("POSTGRES_CONNECTION_STRING")));

builder.Services.Scan(scan => scan
    // .FromAssemblyOf<TokenService>()
    // .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
    // .AsImplementedInterfaces()
    // .WithScopedLifetime()
    .FromAssemblyOf<UserChangeHistoryRepository>()
    .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

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
    
    x.AddConsumer<UserChangeHistoryConsumer>();
    x.AddConsumer<ChangeHistoryConsumer>();
});

builder.Services.AddAutoMapper(cfg => { }, typeof(UserChangeHistoryMapper));
builder.Services.AddAutoMapper(cfg => { }, typeof(ChangeHistoryMapper));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.MaxDepth = 0;
});

SwaggerConfiguration.AddSwaggerOptions(builder);

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    SwaggerConfiguration.AddDevelopSwaggerOptions(builder, app);
}

app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();

app.Run();