using System.Net;
using AuthService.Tests.Factories;
using AuthService.Web.Dto.User;
using General.Auth;
using General.Event.ChangeHistoryService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace AuthService.Tests.IntegrationTests.User;

public partial class UserTest
{
    private UserManager<Core.Entities.User> _userManager;
    private IPasswordHasher<Core.Entities.User> _passwordHasher;

    [SetUp]
    public void Setup()
    {
        _userManager = Scope.ServiceProvider.GetRequiredService<UserManager<Core.Entities.User>>();
        _passwordHasher = Scope.ServiceProvider.GetRequiredService<IPasswordHasher<Core.Entities.User>>();
    }

    [Test]
    public async Task ChangePassword_ShouldReturnOk_WhenValidData()
    {
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var client = CreateAuthenticatedClient(user);
        var changePasswordDto = new ChangePasswordDto(newPassword: "NewSecurePassword123!", UserFactory.DefaultPassword);

        var response = await client.PatchAsJsonAsync($"api/users/{user.Id}/password", changePasswordDto);

        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Неверный статус код ответа");

        var updatedUser = await DbContext.Users.AsNoTracking().IgnoreQueryFilters().Where(u => u.Id == user.Id).FirstOrDefaultAsync();
        
        var passwordVerificationResult = _userManager.PasswordHasher.VerifyHashedPassword(updatedUser, updatedUser.PasswordHash, changePasswordDto.NewPassword);
        Assert.AreEqual(PasswordVerificationResult.Failed, passwordVerificationResult, "Пароль был изменен");
        
        bool sent = await Harness.Published.Any<ChangeHistoryCreateByUserIdRequest>();
        Assert.IsTrue(sent, "Событие ChangeHistoryCreateByUserIdRequest не было отправлено.");
    }

    [Test]
    public async Task ChangePassword_ShouldReturnNotFound_WhenUserNotExists()
    {
        var user = UserFactory.Default(_passwordHasher, DbContext);

        var nonExistentUserId = Guid.NewGuid().ToString();
        var client = CreateAuthenticatedClient(user);

        var changePasswordDto = new ChangePasswordDto(newPassword: "NewSecurePassword123!", UserFactory.DefaultPassword);

        var response = await client.PatchAsJsonAsync($"api/users/{nonExistentUserId}/password", changePasswordDto);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Должна быть ошибка 404 для несуществующего пользователя");
    }

    [Test]
    public async Task ChangePassword_ShouldReturnForbidden_WhenChangingOtherUserPassword()
    {
        var currentUser = UserFactory.Default(_passwordHasher, DbContext);
        var otherUser = UserFactory.Default(_passwordHasher, DbContext);
        var client = CreateAuthenticatedClient(currentUser);
        var changePasswordDto = new ChangePasswordDto(newPassword: "NewSecurePassword123!", UserFactory.DefaultPassword);

        var response = await client.PatchAsJsonAsync($"api/users/{otherUser.Id}/password", changePasswordDto);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Должна быть ошибка 403 при попытке изменить чужой пароль");
    }

    [Test]
    public async Task ChangePassword_ShouldReturnBadRequest_WhenPasswordIsTooShort()
    {
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var client = CreateAuthenticatedClient(user);
        var changePasswordDto = new ChangePasswordDto(newPassword: "123", UserFactory.DefaultPassword);

        var response = await client.PatchAsJsonAsync($"api/users/{user.Id}/password", changePasswordDto);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Должна быть ошибка 400 при слишком коротком пароле");
    }

    [Test]
    public async Task ChangePassword_ShouldWork_WhenUserIsAdmin()
    {
        var adminUser = UserFactory.Default(_passwordHasher, DbContext);
        var regularUser = UserFactory.Default(_passwordHasher, DbContext);
        await _userManager.AddToRoleAsync(adminUser, Roles.RoleAdmin);
        
        var client = CreateAuthenticatedClient(adminUser, new List<string> { Roles.RoleAdmin });
        var changePasswordDto = new ChangePasswordDto(newPassword: "NewAdminPassword123!", UserFactory.DefaultPassword);

        var response = await client.PatchAsJsonAsync($"api/users/{regularUser.Id}/password", changePasswordDto);

        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Админ должен иметь возможность изменить пароль другого пользователя");
    }

    [Test]
    public async Task ChangePassword_ShouldReturnBadRequest_WhenPasswordIsEmpty()
    {
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var client = CreateAuthenticatedClient(user);
        var changePasswordDto = new ChangePasswordDto(newPassword: string.Empty, UserFactory.DefaultPassword);

        var response = await client.PatchAsJsonAsync($"api/users/{user.Id}/password", changePasswordDto);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Должна быть ошибка 400 при пустом пароле");
    }

    [Test]
    public async Task ChangePassword_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var user = UserFactory.Default(_passwordHasher, DbContext);
        var changePasswordDto = new ChangePasswordDto(newPassword: "NewPassword123!", UserFactory.DefaultPassword);

        var response = await Client.PatchAsJsonAsync($"api/users/{user.Id}/password", changePasswordDto);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Должна быть ошибка 401 при отсутствии аутентификации");
    }
}