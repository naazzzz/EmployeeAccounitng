using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using General.Event.ChangeHistoryService;
using General.Event.ProfileService;
using General.Interfaces;
using General.Service;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProfileService.Core.Domain.Entities;
using ProfileService.Core.Mappings.Profile;
using ProfileService.Core.Messaging.Consumers;
using ProfileService.Infrastructure.Repositories;
using Serilog;
using ApplicationContext = ProfileService.Infrastructure.DbContext.ApplicationContext;

namespace ProfileService.Configurations;

public static class ServiceConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.MaxDepth = 0;
        });

        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();
        
        services.Scan(scan => scan
            .FromAssemblyOf<Core.Services.ProfileService>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .FromAssemblyOf<ProfileRepository>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddAutoMapper(cfg => { }, typeof(ProfileMapper));

        Console.WriteLine(configuration.GetValue<string>("POSTGRES_CONNECTION_STRING"));
        
        services.AddDbContext<ApplicationContext>((serviceProvider, options) =>
            options.UseNpgsql(configuration.GetValue<string>("POSTGRES_CONNECTION_STRING")));

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationContext>()
            .AddDefaultTokenProviders();
        
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = configuration.GetValue<string>("AUTH_SERVICE_ADDRESS");
                options.Audience = configuration.GetValue<string>("JWT_AUD_CLAIM");
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Log.Error(context.Exception, "JWT Authentication failed: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Log.Information("JWT Token validated: {UserName}", context.Principal?.Identity?.Name);
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("DefaultWithNotConfirmedMail",
                policy => policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser());
        });

        services.AddMassTransit(x =>
        {
            var brokerType = Environment.GetEnvironmentVariable("BROKER_TYPE") ?? "RabbitMQ";
            var brokerHost = Environment.GetEnvironmentVariable("BROKER_HOST") ?? "localhost";
        
            if (brokerType == "RabbitMQ")
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(brokerHost, h =>
                    {
                        h.Username("kalo");
                        h.Password("kalo");
                    });
                    
                    cfg.ConfigureEndpoints(context);
                });

            x.AddConsumer<ProfileResponseConsumer>();
            x.AddRequestClient<ChangeHistoryRequest>();
        });
    }
}