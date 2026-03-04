using System.Security.Authentication;
using System.Text.Json.Serialization;
using AuthService.Core.Entities;
using AuthService.Core.Events.Listeners;
using AuthService.Core.Mappings.User;
using AuthService.Infrastructure.DbContext;
using AuthService.Infrastructure.HostedServices;
using AuthService.Infrastructure.Interceptors;
using AuthService.Infrastructure.Messaging.Consumers;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Seeder;
using DotNetEnv;
using DotNetEnv.Configuration;
using General.Configurations;
using General.Event.ChangeHistoryService;
using General.Interfaces;
using General.Service;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Serilog;
using AuthenticationMiddleware = AuthService.Web.Middlewares.AuthenticationMiddleware;

Env.Load();

var builder = WebApplication.CreateBuilder(args);


builder.Logging.AddConsole();

IConfigurationRoot configuration = builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath()).Build();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.MaxDepth = 0;
});

builder.Services.AddHostedService<ClearExpireMailCodesTimed>();
builder.Services.AddHostedService<NotificationExpirePasswordTimed>();

builder.Services.AddAutoMapper(cfg => { }, typeof(UserMapper));

builder.Services.AddScoped<IPasswordHasher<object>, PasswordHasher<object>>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddHttpClient("ProfileService",
    client =>
    {
        client.BaseAddress = new Uri(configuration.GetValue<string>("USER_SERVICE_URI") ??
                                     throw new InvalidOperationException());
        client.DefaultRequestHeaders.UserAgent.ParseAdd("dotnet-docs");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    }).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        SslProtocols = SslProtocols.Tls12 |
                       SslProtocols.Tls13
    };
});

builder.Services.AddScoped<UserInterceptor>();
builder.Services.AddScoped<SoftDeleteInterceptor>();

builder.Services.AddHttpContextAccessor();

// Register AuditListener as scoped to match the lifetime of ApplicationContext
builder.Services.AddScoped<AuditListener>();

if (builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<ApplicationContext>((serviceProvider, options) =>
        {
            options.UseInMemoryDatabase("TestDatabase")
                .AddInterceptors(serviceProvider.GetRequiredService<SoftDeleteInterceptor>())
                .AddInterceptors(serviceProvider.GetRequiredService<UserInterceptor>())
                .UseOpenIddict();
        }
    );
}
else
{
    builder.Services.AddDbContext<ApplicationContext>((serviceProvider, options) =>
    {
        options.UseNpgsql(configuration.GetValue<string>("POSTGRES_CONNECTION_STRING"))
            .AddInterceptors(serviceProvider.GetRequiredService<SoftDeleteInterceptor>())
            .AddInterceptors(serviceProvider.GetRequiredService<UserInterceptor>())
            .UseOpenIddict();
    });
}

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationContext>();
    })
    .AddServer(options =>
    {
        // Enable the token endpoint.
        options.SetTokenEndpointUris("connect/token");

        options.AllowClientCredentialsFlow().AllowRefreshTokenFlow();
        options.AllowPasswordFlow().AllowRefreshTokenFlow();

        // Encryption and signing of tokens
        options
            .AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate()
            .DisableAccessTokenEncryption();

        // Register the ASP.NET Core host and configure the ASP.NET Core options.
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .DisableTransportSecurityRequirement();
    });

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationContext>()
    .AddRoles<IdentityRole>()
    .AddDefaultTokenProviders();

builder.Services.Scan(scan => scan
    .FromAssemblyOf<AuthService.Core.Services.User.UserService>()
    .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
    .AsImplementedInterfaces()
    .WithScopedLifetime()
    .FromAssemblyOf<UserRepository>()
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

    x.AddConsumer<ConfirmationCodeConsumer>();
    x.AddRequestClient<ChangeHistoryRequest>();
    x.AddRequestClient<ChangeHistoryCreateByUserIdRequest>();
    x.AddRequestClient<ChangeHistoryRequestLatestByUserId>();
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = configuration.GetValue<string>("BASE_HTTPS_URL");
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DefaultWithNotConfirmedMail",
        policy => policy.RequireAuthenticatedUser());
});

SwaggerConfiguration.AddSwaggerOptions(builder);
RateLimiterConfiguration.Configure(builder.Services);

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

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
}

app.UseDeveloperExceptionPage();
app.UseForwardedHeaders();
app.UseRouting();
app.UseRateLimiter();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuthenticationMiddleware>();
app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();