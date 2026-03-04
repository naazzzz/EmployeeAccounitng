using System.Net;
using System.Text.Json;
using AuthService.Tests.Factories;
using AuthService.Web.Dto.MailCode;
using General.Dto;
using General.Event.ChangeHistoryService;
using General.Event.NotificationService;
using General.ValueObjects.Enums;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace AuthService.Tests.IntegrationTests.Codes;

public partial class CodeTest
{
    [Test]
    public async Task CreateNewCode_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var client = CreateAuthenticatedClient(user);
        
        var changeHistoryId = Guid.NewGuid().ToString();
        var code = CodeFactory.Default(user.Id, changeHistoryId, DbContext);
        
        TestChangeHistoryConsumer.MockResponse = new ChangeHistoryResponse(new UserChangeHistoryDto
        {
            Id = changeHistoryId,
            UserId = user.Id,
            ActionEnum = HistoryActionEnum.ChangeAuthDataRequest,
            ValueAction = JsonSerializer.Serialize(user)
        });

        // Act
        var response = await client.PostAsJsonAsync("api/codes", new CreateCodeDto(user.Id,code.RefreshCode));

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("Код подтверждения отправлен на почту"));
        
        // Verify code was created in database
        var createdCode = await DbContext.ConfirmationCode
            .FirstOrDefaultAsync(c => c.UserId == user.Id && c.Id != code.Id);
        Assert.IsNotNull(createdCode);
        
        // Verify event was published
        Assert.IsTrue(await Harness.Published.Any<CodeSentEvent>());
    }

    [Test]
    public async Task CreateNewCode_ShouldReturnOk_WhenNoRefreshCodeProvided()
    {
        // Arrange
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var client = CreateAuthenticatedClient(user);
        
        var changeHistoryId = Guid.NewGuid().ToString();
        var code = CodeFactory.Default(user.Id, changeHistoryId, DbContext);
        
        TestChangeHistoryConsumer.MockResponse = new ChangeHistoryResponse(new UserChangeHistoryDto
        {
            Id = changeHistoryId,
            UserId = user.Id,
            ActionEnum = HistoryActionEnum.ChangeAuthDataRequest,
            ValueAction = JsonSerializer.Serialize(user)
        });

        // Act
        var response = await client.PostAsJsonAsync("api/codes", new CreateCodeDto(user.Id, null));

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseContent.Contains("Код подтверждения отправлен на почту"));
        
        Assert.IsTrue(await Harness.Published.Any<ChangeHistoryRequestLatestByUserId>(), "Событие ChangeHistoryRequest не было отправлено.");
        Assert.IsTrue(await Harness.Consumed.Any<ChangeHistoryRequestLatestByUserId>(), "Событие ChangeHistoryRequest не было отправлено.");
        
        // Verify code was created in database
        var createdCode = await DbContext.ConfirmationCode
            .FirstOrDefaultAsync(c => c.UserId == user.Id && c.Id != code.Id);
        Assert.IsNotNull(createdCode);
        
        // Verify event was published
        Assert.IsTrue(await Harness.Published.Any<CodeSentEvent>());
    }

    [Test]
    public async Task CreateNewCode_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var nonExistentUserId = Guid.NewGuid().ToString();
        var client = CreateAuthenticatedClient(user);

        // Act
        var response = await client.PostAsJsonAsync("api/codes", new CreateCodeDto(nonExistentUserId, "some-refresh-code"));

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public async Task CreateNewCode_ShouldReturnNotFound_WhenRefreshCodeIsInvalid()
    {
        // Arrange
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var client = CreateAuthenticatedClient(user);

        // Act
        var response = await client.PostAsJsonAsync("api/codes", new CreateCodeDto(user.Id, "invalid-refresh-code"));

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}