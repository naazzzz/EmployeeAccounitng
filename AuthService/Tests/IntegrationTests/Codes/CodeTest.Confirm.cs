using System.Net;
using System.Text.Json;
using AuthService.Tests.Factories;
using AuthService.Web.Dto.MailCode;
using General.Dto;
using General.Event.ChangeHistoryService;
using General.ValueObjects.Enums;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace AuthService.Tests.IntegrationTests.Codes;

[TestFixture]
[TestOf(typeof(CodeTest))]
public partial class CodeTest: BaseTest
{
    private UserManager<Core.Entities.User> _userManager;
    private IPasswordHasher<Core.Entities.User> _passwordHasher;

    [SetUp]
    public void Setup()
    {
        _userManager = Scope.ServiceProvider.GetRequiredService<UserManager<Core.Entities.User>>();
        _passwordHasher = Scope.ServiceProvider.GetRequiredService<IPasswordHasher<Core.Entities.User>>();
    }
    
    protected override void ConfigureMassTransit(IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<TestChangeHistoryConsumer>();
        
        configurator.AddRequestClient<ChangeHistoryRequest>();
        configurator.AddRequestClient<ChangeHistoryRequestLatestByUserId>();
        
        configurator.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    }
    
    private class TestChangeHistoryConsumer : IConsumer<ChangeHistoryRequest>, IConsumer<ChangeHistoryRequestLatestByUserId>
    {
        public static ChangeHistoryResponse MockResponse { get; set; }

        public Task Consume(ConsumeContext<ChangeHistoryRequest> context)
        {
            return context.RespondAsync(MockResponse);
        }
        
        public Task Consume(ConsumeContext<ChangeHistoryRequestLatestByUserId> context)
        {
            return context.RespondAsync(MockResponse);
        }
    }
    
    [Test]
    public async Task Confirm_ShouldReturnOk_WhenCodeExistsAndNotExpired()
    {
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var changeHistoryId = Guid.NewGuid().ToString();
        var code = CodeFactory.Default(user.Id, changeHistoryId, DbContext);
        
        var client = CreateAuthenticatedClient(user);

        var cloneUser = user.CloneShallow();
        cloneUser.Email = "new.email@example.com";
        
        TestChangeHistoryConsumer.MockResponse = new ChangeHistoryResponse(new UserChangeHistoryDto(){
            UserId = user.Id,
            ValueAction = JsonSerializer.Serialize(cloneUser),
            Id = changeHistoryId,
            ActionEnum = HistoryActionEnum.ChangeAuthDataRequest
        });
        
        var response = await client.PatchAsJsonAsync($"api/codes/confirm", new ConfirmMailDto(
            code: code.Code,
            userId: user.Id
            ));
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Должна вернуться 200 при подтверждении кода");
        
        Assert.IsTrue(await Harness.Published.Any<ChangeHistoryRequest>(), "Событие ChangeHistoryRequest не было отправлено.");
        Assert.IsTrue(await Harness.Consumed.Any<ChangeHistoryRequest>(), "Событие ChangeHistoryRequest не было отправлено.");
        
        var updatedUser =await DbContext.Users.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.AreEqual(updatedUser.Email, cloneUser.Email, "Email должен измениться");
    }
    
    [Test]
    public async Task Confirm_ShouldReturnOk_WhenCodeExistsAndNotExpired_ForUserWithNotConfirmedEmail()
    {
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var changeHistoryId = Guid.NewGuid().ToString();
        var code = CodeFactory.Default(user.Id, changeHistoryId, DbContext);
        user.EmailConfirmed = false;
        
        var client = CreateAuthenticatedClient(user);

        var cloneUser = user.CloneShallow();
        cloneUser.EmailConfirmed = true;
        
        TestChangeHistoryConsumer.MockResponse = new ChangeHistoryResponse(new UserChangeHistoryDto(){
            UserId = user.Id,
            ValueAction = JsonSerializer.Serialize(cloneUser),
            Id = changeHistoryId,
            ActionEnum = HistoryActionEnum.ChangeAuthDataRequest
        });
        
        var response = await client.PatchAsJsonAsync($"api/codes/confirm", new ConfirmMailDto(
            code: code.Code,
            userId: user.Id
        ));
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Должна вернуться 200 при подтверждении кода");
        
        Assert.IsTrue(await Harness.Published.Any<ChangeHistoryRequest>(), "Событие ChangeHistoryRequest не было отправлено.");
        Assert.IsTrue(await Harness.Consumed.Any<ChangeHistoryRequest>(), "Событие ChangeHistoryRequest не было отправлено.");
        
        var updatedUser =await DbContext.Users.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.AreEqual(updatedUser.Email, cloneUser.Email, "Email должен измениться");
    }
    
    [Test]
    public async Task Confirm2FaChanges_ShouldReturnNotFound_WhenCodeIsInvalid()
    {
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var changeHistoryId = Guid.NewGuid().ToString();
        var code = CodeFactory.Default(user.Id, changeHistoryId, DbContext);
        
        var client = CreateAuthenticatedClient(user);

        var cloneUser = user.CloneShallow();
        cloneUser.Email = "new.email@example.com";
        
        TestChangeHistoryConsumer.MockResponse = new ChangeHistoryResponse(new UserChangeHistoryDto(){
            UserId = user.Id,
            ValueAction = JsonSerializer.Serialize(cloneUser),
            Id = changeHistoryId,
            ActionEnum = HistoryActionEnum.ChangeAuthDataRequest
        });
        
        var dto = new ConfirmMailDto("invalid-code", "non-existent-user-id");
        
        var response = await client.PatchAsJsonAsync($"api/codes/confirm", dto);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Test]
    public async Task Confirm2FaChanges_ShouldReturnNotFound_WhenUserNotFound()
    {
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var changeHistoryId = Guid.NewGuid().ToString();
        var code = CodeFactory.Default(user.Id, changeHistoryId, DbContext);
        
        var client = CreateAuthenticatedClient(user);

        var cloneUser = user.CloneShallow();
        cloneUser.Email = "new.email@example.com";
        
        TestChangeHistoryConsumer.MockResponse = new ChangeHistoryResponse(new UserChangeHistoryDto(){
            UserId = user.Id,
            ValueAction = JsonSerializer.Serialize(cloneUser),
            Id = changeHistoryId,
            ActionEnum = HistoryActionEnum.ChangeAuthDataRequest
        });
        
        var dto = new ConfirmMailDto(code.Code, "non-existent-user-id");
        
        var response = await client.PatchAsJsonAsync($"api/codes/confirm", dto);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}