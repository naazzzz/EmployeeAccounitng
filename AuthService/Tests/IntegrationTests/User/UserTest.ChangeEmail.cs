using System.Net;
using AuthService.Tests.Factories;
using General.Event.ChangeHistoryService;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using UserService.Web.Dto.User;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace AuthService.Tests.IntegrationTests.User;

public partial class UserTest
{
    [Test]
    public async Task ChangeEmail_ShouldReturnOk_WhenValidData()
    {
        // Arrange
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var client = CreateAuthenticatedClient(user);
        var changeEmailDto = new ChangeEmailDto("new.email@example.com");
        var originalEmail = user.Email;

        // Act
        var response = await client.PatchAsJsonAsync($"api/users/{user.Id}/email", changeEmailDto);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Неверный статус код ответа");

        // Verify email wasn't changed (since it requires confirmation)
        var updatedUser = await DbContext.Users
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == user.Id);
            
        Assert.AreEqual(originalEmail, updatedUser.Email, "Email не должен измениться до подтверждения");
        
        // Verify that ChangeHistoryCreateByUserIdRequest was published
        bool sent = await Harness.Published.Any<ChangeHistoryCreateByUserIdRequest>();
        Assert.IsTrue(sent, "Событие ChangeHistoryCreateByUserIdRequest не было отправлено.");
    }

    [Test]
    public async Task ChangeEmail_ShouldReturnNotFound_WhenUserNotExists()
    {
        var user = UserFactory.Default(_passwordHasher, DbContext);

        var nonExistentUserId = Guid.NewGuid().ToString();
        var client = CreateAuthenticatedClient(user);
        var changeEmailDto = new ChangeEmailDto("new.email@example.com");

        // Act
        var response = await client.PatchAsJsonAsync($"api/users/{nonExistentUserId}/email", changeEmailDto);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Должна вернуться 404 для несуществующего пользователя");
    }

    [Test]
    public async Task ChangeEmail_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var changeEmailDto = new ChangeEmailDto("new.email@example.com");

        // Act
        var response = await Client.PatchAsJsonAsync($"api/users/{user.Id}/email", changeEmailDto);

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Должна вернуться 401 для неаутентифицированного пользователя");
    }

    [Test]
    public async Task ChangeEmail_ShouldReturnBadRequest_WhenModelIsInvalid()
    {
        // Arrange
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var client = CreateAuthenticatedClient(user);
        var invalidEmailDto = new ChangeEmailDto("not-an-email");

        // Act
        var response = await client.PatchAsJsonAsync($"api/users/{user.Id}/email", invalidEmailDto);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Должна вернуться 400 при невалидной модели");
    }
}
