using AuthService.Core.Entities;
using Bogus;
using DotNetEnv;
using DotNetEnv.Configuration;
using General.Dto;
using General.Event.AuthService;
using General.Event.ChangeHistoryService;
using General.ValueObjects.Enums;
using HistoryService.Core.Domain.Entity;
using HistoryService.Core.Interfaces;
using HistoryService.Core.Mappings.ChangeHistory;
using HistoryService.Infrastructure;
using HistoryService.Infrastructure.Messaging.Consumers;
using HistoryService.Infrastructure.Repositories;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace HistoryService.Tests.IntegrationTests.Consumers;

[TestFixture]
[TestOf(typeof(UserChangeHistoryConsumerTest))]
public class UserChangeHistoryConsumerTest
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
                options.UseInMemoryDatabase("TestDatabase"))
            .AddScoped<IUserChangeHistoryRepository, UserChangeHistoryRepository>()
            .AddLogging(logging => logging.AddConsole())
            .AddAutoMapper(cfg => { }, typeof(UserChangeHistoryMapper))
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<UserChangeHistoryConsumer>();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ReceiveEndpoint("change-history-queue", e =>
                    {
                        Console.WriteLine("Configuring ChangeHistoryConsumer endpoint");
                        e.Consumer<UserChangeHistoryConsumer>(context);
                    });
                });
            })
            .AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationContext>()
            .AddDefaultTokenProviders().Services;

        _provider = services.BuildServiceProvider(true);
        _harness = _provider.GetRequiredService<ITestHarness>();
        
        _scope = _provider.CreateScope();
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

    public static Faker<UserChangeHistoryDto> Default()
    {
        return new Faker<UserChangeHistoryDto>()
            .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
            .RuleFor(u => u.CreatedAt, f => f.Date.PastOffset(1, DateTimeOffset.UtcNow))
            .RuleFor(u => u.ActionEnum, f => f.PickRandom<HistoryActionEnum>())
            .RuleFor(u => u.ValueAction, f => JsonSerializer.Serialize(
                new
                {
                    UserName = "test",
                    Email = "test@email.ru"
                }
            ))
            .RuleFor(u => u.UserId, f => Guid.NewGuid().ToString());
    }

    [Test]
    public async Task Consume_ChangeHistoryCreateByUserIdRequest_SaveChangeHistoryAndSendNotification()
    {
        await _harness.Start();

        var changeHistoryDto = Default().Generate();
        changeHistoryDto.NeedCode = true;

        await _harness.Bus.Publish(new ChangeHistoryCreateByUserIdRequest(changeHistoryDto));

        bool consumed = await _harness.Consumed.Any<ChangeHistoryCreateByUserIdRequest>();
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(consumed, "Событие ChangeHistoryCreateByUserIdRequest не было обработано.");

        bool sent = await _harness.Published.Any<CreateConfirmationCodeEvent>();
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(sent, "Событие CreateConfirmationCode не было отправлено.");
        
        var savedChangeHistory = await _dbContext.UserChangeHistory
            .FirstOrDefaultAsync(ch => ch.UserId == changeHistoryDto.UserId);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(savedChangeHistory, "Запись в базе данных не создана.");
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(changeHistoryDto.ValueAction, savedChangeHistory.ValueAction);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(changeHistoryDto.ActionEnum, savedChangeHistory.ActionEnum);
    }

    [Test]
    public async Task Consume_ChangeHistoryRequest_GetChangeHistory()
    {
        await _harness.Start();

        var changeHistoryDto = Default().Generate();

        await _dbContext.UserChangeHistory.AddAsync(new UserChangeHistory()
        {
            Id = changeHistoryDto.Id,
            UserId = changeHistoryDto.UserId,
            ValueAction = changeHistoryDto.ValueAction,
            ActionEnum = changeHistoryDto.ActionEnum,
            CreatedAt = changeHistoryDto.CreatedAt
        });
        await _dbContext.SaveChangesAsync();

        var client = _harness.GetRequestClient<ChangeHistoryRequest>();

        var response = await client.GetResponse<ChangeHistoryResponse>(new ChangeHistoryRequest(changeHistoryDto.Id));

        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(
            JsonConvert.SerializeObject(response.Message.UserChangeHistoryDto),
            JsonConvert.SerializeObject(changeHistoryDto));
    }
    
    [Test]
    public async Task Consume_ChangeHistoryRequestLatestByUserId_GetLatestChangeHistory()
    {
        await _harness.Start();

        var changeHistoryDto = Default().Generate();

        await _dbContext.UserChangeHistory.AddAsync(new UserChangeHistory()
        {
            Id = changeHistoryDto.Id,
            UserId = changeHistoryDto.UserId,
            ValueAction = changeHistoryDto.ValueAction,
            ActionEnum = changeHistoryDto.ActionEnum,
            CreatedAt = changeHistoryDto.CreatedAt
        });
        
        await _dbContext.UserChangeHistory.AddAsync(new UserChangeHistory()
        {
            UserId = changeHistoryDto.UserId,
            ValueAction = changeHistoryDto.ValueAction,
            ActionEnum = changeHistoryDto.ActionEnum,
            CreatedAt = changeHistoryDto.CreatedAt.AddHours(-1)
        });
        
        await _dbContext.UserChangeHistory.AddAsync(new UserChangeHistory()
        {
            UserId = "test",
            ValueAction = changeHistoryDto.ValueAction,
            ActionEnum = changeHistoryDto.ActionEnum,
            CreatedAt = changeHistoryDto.CreatedAt
        });
        
        await _dbContext.UserChangeHistory.AddAsync(new UserChangeHistory()
        {
            UserId = "test",
            ValueAction = changeHistoryDto.ValueAction,
            ActionEnum = changeHistoryDto.ActionEnum,
            CreatedAt = changeHistoryDto.CreatedAt
        });
        
        await _dbContext.SaveChangesAsync();

        var client = _harness.GetRequestClient<ChangeHistoryRequestLatestByUserId>();

        var response = await client.GetResponse<ChangeHistoryResponse>(new ChangeHistoryRequestLatestByUserId(changeHistoryDto.UserId));

        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(
            JsonConvert.SerializeObject(response.Message.UserChangeHistoryDto),
            JsonConvert.SerializeObject(changeHistoryDto));
    }
}