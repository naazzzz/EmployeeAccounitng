using General.Event.NotificationService;
using MassTransit;
using MassTransit.Testing;
using MimeKit;
using Moq;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Messaging.Consumers;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace NotificationService.Tests.UnitTests.Consumers;

[TestFixture]
[TestOf(typeof(NotificationConsumerTest))]
public class NotificationConsumerTest
{
    private Mock<ISmtpClient> _mockSmtpClient;
    private ITestHarness _harness;

    [SetUp]
    public void SetUp()
    {
        _mockSmtpClient = new Mock<ISmtpClient>(MockBehavior.Loose);

        var services = new ServiceCollection()
            .AddSingleton(_mockSmtpClient.Object)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<NotificationConsumer>();
            });

        var provider = services.BuildServiceProvider();
        _harness = provider.GetRequiredService<ITestHarness>();
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await _harness.Stop();
    }
    
    [Test]
    public async Task Consume_UserTransferredEvent_SendsEmailWithCorrectParameters()
    {
        const string testUsername = "test";
        const string testEmail = "test@email.ru";
        const string testDepartmentName = "DefaultTest";
        const string expectedSubject = "Notification DBI Message";
        var expectedBody = $"Вы были переведены в новый департамент - {testDepartmentName}";

        _mockSmtpClient
            .Setup(client => client.SendMessageAsync(
                It.Is<MailboxAddress>(addr => addr.Name == testUsername && addr.Address == testEmail),
                false,
                expectedBody,
                expectedSubject))
            .Returns(Task.CompletedTask);

        await _harness.Start();
        
        await _harness.Bus.Publish(new UserTransferredEvent
        {
            DepartmentName = testDepartmentName,
            UserName = testUsername,
            Email = testEmail
        });

        bool consumed = await _harness.Consumed.Any<UserTransferredEvent>();
        Assert.IsTrue(consumed, "Событие UserTransferredEvent не было обработано.");

        _mockSmtpClient.Verify(
            client => client.SendMessageAsync(
                It.Is<MailboxAddress>(addr => addr.Name == testUsername && addr.Address == testEmail),
                false,
                expectedBody,
                expectedSubject),
            Times.Once(),
            "Метод SendMessageAsync должен быть вызван один раз с правильными параметрами.");
    }
    
    [Test]
    public async Task Consume_CodeSentEvent_SendsEmailWithCorrectParameters()
    {
        const string testUsername = "test";
        const string testEmail = "test@email.ru";
        const string testCode = "testCode";
        const string expectedSubject = "2FA DBI Message";

        _mockSmtpClient
            .Setup(client => client.SendMessageAsync(
                It.Is<MailboxAddress>(addr => addr.Name == testUsername && addr.Address == testEmail),
                false,
                testCode,
                expectedSubject))
            .Returns(Task.CompletedTask);
        
        await _harness.Start();
        
        await _harness.Bus.Publish(new CodeSentEvent
        {
            Code = testCode,
            UserName = testUsername,
            Email = testEmail
        });

        bool consumed = await _harness.Consumed.Any<CodeSentEvent>();
        Assert.IsTrue(consumed, "Событие CodeSentEvent не было обработано.");

        _mockSmtpClient.Verify(
            client => client.SendMessageAsync(
                It.Is<MailboxAddress>(addr => addr.Name == testUsername && addr.Address == testEmail),
                false,
                testCode,
                expectedSubject),
            Times.Once(),
            "Метод SendMessageAsync должен быть вызван один раз с правильными параметрами.");
    }
    
    [Test]
    public async Task Consume_PasswordExpiringEvent_SendsEmailWithCorrectParameters()
    {
        const string testUsername = "test";
        const string testEmail = "test@email.ru";
        const string expectedSubject = "Notification DBI Message";
        const string expectedMessage =  "Истекает срок действия вашего пароля.";
        
        _mockSmtpClient
            .Setup(client => client.SendMessageAsync(
                It.Is<MailboxAddress>(addr => addr.Name == testUsername && addr.Address == testEmail),
                false,
                expectedMessage,
                expectedSubject))
            .Returns(Task.CompletedTask);
        
        await _harness.Start();
        
        await _harness.Bus.Publish(new PasswordExpiringEvent
        {
            UserName = testUsername,
            Email = testEmail
        });

        bool consumed = await _harness.Consumed.Any<PasswordExpiringEvent>();
        Assert.IsTrue(consumed, "Событие PasswordExpiringEvent не было обработано.");

        _mockSmtpClient.Verify(
            client => client.SendMessageAsync(
                It.Is<MailboxAddress>(addr => addr.Name == testUsername && addr.Address == testEmail),
                false,
                expectedMessage,
                expectedSubject),
            Times.Once(),
            "Метод SendMessageAsync должен быть вызван один раз с правильными параметрами.");
    }
}