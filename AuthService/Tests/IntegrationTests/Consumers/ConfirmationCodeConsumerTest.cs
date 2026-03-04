using AuthService.Core.Entities;
using AuthService.Core.Mappings.User;
using AuthService.Infrastructure.DbContext;
using AuthService.Infrastructure.Messaging.Consumers;
using AuthService.Infrastructure.Repositories;
using AuthService.Tests.Factories;
using DotNetEnv;
using DotNetEnv.Configuration;
using General.Event.AuthService;
using General.Event.NotificationService;
using General.Interfaces;
using General.Service;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace AuthService.Tests.IntegrationTests.Consumers;

[TestFixture]
[TestOf(typeof(ConfirmationCodeConsumerTest))]
public class ConfirmationCodeConsumerTest
{
    private ITestHarness _harness;
    private ServiceProvider _provider;
    private ApplicationContext _dbContext;
    private IServiceScope _scope;

    [SetUp]
    public void SetUp()
    {
        var configuration = new ConfigurationBuilder().AddDotNetEnv(".env", LoadOptions.TraversePath()).Build();

        var services = new ServiceCollection()
            .AddDbContext<ApplicationContext>((serviceProvider, options) =>
                options.UseInMemoryDatabase("TestDatabase"), ServiceLifetime.Singleton)
            .AddLogging(logging => logging.AddConsole())
            .AddAutoMapper(cfg => { }, typeof(UserMapper))
            .AddSingleton<ICurrentUserService, CurrentUserService>()
            .AddIdentity<Core.Entities.User, IdentityRole>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationContext>().Services
            .Scan(scan => scan
                .FromAssemblyOf<Core.Services.User.UserService>()
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .FromAssemblyOf<UserRepository>()
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
                .AsImplementedInterfaces()
                .WithScopedLifetime())
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<ConfirmationCodeConsumer>();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ReceiveEndpoint("confirmation-code-queue", e =>
                    {
                        Console.WriteLine("Configuring ConfirmationCodeConsumer endpoint");
                        e.Consumer<ConfirmationCodeConsumer>(context);
                    });
                });
                
            })
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
            .AddScoped<IPasswordHasher<Core.Entities.User>, PasswordHasher<Core.Entities.User>>();

        _provider = services.BuildServiceProvider(true);
        _scope = _provider.CreateScope();
        
        _harness = _scope.ServiceProvider.GetRequiredService<ITestHarness>();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeletedAsync();
        _harness.Stop();
        _scope.Dispose();
        _provider.DisposeAsync();
    }

    [Test]
    public async Task Consume_CreateConfirmationCode_CreateNewCodeAndSendNotification()
    {
        await _harness.Start();

        var user = UserFactory.Default(_scope.ServiceProvider.GetRequiredService<IPasswordHasher<Core.Entities.User>>());
        user.TwoFactorEnabled = false;
        
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var userChangeHistoryId = Guid.NewGuid().ToString();
        
        await _harness.Bus.Publish(new CreateConfirmationCodeEvent()
        {
            UserId = user.Id,
            UserChangeHistoryId = userChangeHistoryId
        });

        bool consumed = await _harness.Consumed.Any<CreateConfirmationCodeEvent>();
        Assert.IsTrue(consumed, "Событие CreateConfirmationCode не было обработано.");

        bool sent = await _harness.Published.Any<CodeSentEvent>();
        Assert.IsTrue(sent, "Событие CodeSentEvent не было отправлено.");
        
        var savedChangeHistory = await _dbContext.ConfirmationCode
            .LastOrDefaultAsync();
        Assert.IsNotNull(savedChangeHistory, "Запись в базе данных не создана.");
        Assert.AreEqual(savedChangeHistory.UserId, user.Id);
    }
}