using AuthService.Core.Entities;
using AuthService.Infrastructure.DbContext;
using AuthService.Tests.Helpers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;

namespace AuthService.Tests;

public abstract class BaseTest
{
    private WebApplicationFactory<Program> Factory { get; set; }
    protected HttpClient Client { get; private set; }
    protected IServiceProvider Provider { get; private set; }
    protected IServiceScope Scope { get; private set; }
    protected ApplicationContext DbContext { get; private set; }
    protected ITestHarness Harness { get; private set; }

    [SetUp]
    public virtual void SetUp()
    {
        // todo проверить, если нужно добавить .env.test
        // Загружаем конфигурацию из .env
        // var configuration = new ConfigurationBuilder()
        //     .AddDotNetEnv(".env.test", LoadOptions.TraversePath())
        //     .Build();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                ConfigureWebHost(builder);
            });

        Client = Factory.CreateClient();

        Provider = Factory.Services;
        Scope = Provider.CreateScope();

        DbContext = Scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        Harness = Factory.Services.GetTestHarness();

        Harness.Start();

        DbContext.Database.EnsureCreated();
        // DbContext.InitializeTestData();
    }

    [TearDown]
    public virtual async Task TearDown()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await Harness.Stop();
        Scope.Dispose();
        Client.Dispose();
        Factory.Dispose();
    }

    protected virtual void ConfigureMassTransit(IBusRegistrationConfigurator configurator)
    {
        configurator.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    }
    
    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = TestAuthHandler.TestScheme;
            o.DefaultChallengeScheme = TestAuthHandler.TestScheme;
        });

        services.RemoveAll<IAuthenticationHandler>();
        services.RemoveAll<IAuthenticationSchemeProvider>();
        
        services.AddAuthentication(TestAuthHandler.TestScheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.TestScheme, options => { });
    }
    
    protected virtual void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var massTransitHostedService = services.FirstOrDefault(d => d.ServiceType == typeof(IHostedService) &&
                                                                        d.ImplementationFactory != null &&
                                                                        d.ImplementationFactory.Method.ReturnType == typeof(MassTransitHostedService)
            );
            
            services.Remove(massTransitHostedService);
            
            var descriptors = services.Where(d => 
                    d.ServiceType.Namespace.Contains("MassTransit",StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            foreach (var d in descriptors) 
            {
                services.Remove(d);
            }    
            
            services.AddMassTransitTestHarness(x =>
            {
                ConfigureMassTransit(x);
            });
        });
        
        builder.UseEnvironment("Test");
    }
    
    protected virtual HttpClient CreateAuthenticatedClient(
        User user,
        IEnumerable<string>? roles = null
        )
    {
        var client = Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    ConfigureTestServices(services);
                    services.Configure<TestAuthHandlerOptions>(options =>
                    {
                        options.UserId = user.Id;
                        options.UserEmail = user.Email;
                        options.UserName = user.UserName;
                        options.Roles = roles;
                    });
                    
                    // КОНСТРУКЦИЯ ДЛЯ ЗАМЕНЕНЫ BUS НА ТОТ КОТОТРЫЙ В БАЗОВОМ ТЕСТЕ 
                    // ДЛЯ ПРОВЕРКИ ОПУБЛИКОВАННЫХ СООБЩЕНИЙ
                    services.RemoveAll<IBus>();
                    services.AddSingleton<IBus>(_ => Harness.Bus);
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });

        return client;
    }
}