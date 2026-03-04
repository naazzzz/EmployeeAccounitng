using System.Net;
using System.Threading.Tasks;
using AuthService.Tests.Factories;
using General.Auth;
using General.Event.ChangeHistoryService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace AuthService.Tests.IntegrationTests.User;

public partial class UserTest
{
    [Test]
    public async Task BlockUser_ShouldReturnOk_WhenUserExistsAndAdmin()
    {
        // Arrange
        var adminUser = UserFactory.Default(_passwordHasher, DbContext);
        var regularUser = UserFactory.Default(_passwordHasher, DbContext);
        await _userManager.AddToRoleAsync(adminUser, Roles.RoleAdmin);
        
        var client = CreateAuthenticatedClient(adminUser, new List<string> { Roles.RoleAdmin });

        // Act
        var response = await client.PatchAsync($"api/users/{regularUser.Id}/block", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Неверный статус код ответа");

        // Verify user is blocked
        var blockedUser = await DbContext.Users
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == regularUser.Id);
            
        Assert.IsTrue(blockedUser.IsBlocked, "Пользователь должен быть заблокирован");
        
        // Verify that ChangeHistoryCreateByUserIdRequest was published
        bool sent = await Harness.Published.Any<ChangeHistoryCreateByUserIdRequest>();
        Assert.IsTrue(sent, "Событие ChangeHistoryCreateByUserIdRequest не было отправлено.");
    }

    [Test]
    public async Task BlockUser_ShouldReturnForbidden_WhenNotAdmin()
    {
        // Arrange
        var regularUser = UserFactory.Default(_passwordHasher, DbContext);
        var anotherUser = UserFactory.Default(_passwordHasher, DbContext);
        var client = CreateAuthenticatedClient(regularUser);

        // Act
        var response = await client.PatchAsync($"api/users/{anotherUser.Id}/block", null);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "Должна вернуться 403 для пользователя без прав администратора");
    }

    [Test]
    public async Task BlockUser_ShouldReturnNotFound_WhenUserNotExists()
    {
        // Arrange
        var adminUser = UserFactory.Default(_passwordHasher, DbContext);
        await _userManager.AddToRoleAsync(adminUser, Roles.RoleAdmin);
        var client = CreateAuthenticatedClient(adminUser, new List<string> { Roles.RoleAdmin });
        var nonExistentUserId = Guid.NewGuid().ToString();

        // Act
        var response = await client.PatchAsync($"api/users/{nonExistentUserId}/block", null);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Должна вернуться 404 для несуществующего пользователя");
    }

    [Test]
    public async Task UnblockUser_ShouldReturnOk_WhenUserExistsAndAdmin()
    {
        // Arrange
        var adminUser = UserFactory.Default(_passwordHasher, DbContext);
        var blockedUser = UserFactory.Default(_passwordHasher, DbContext);
        blockedUser.IsBlocked = true;
        await DbContext.SaveChangesAsync();
        
        await _userManager.AddToRoleAsync(adminUser, Roles.RoleAdmin);
        var client = CreateAuthenticatedClient(adminUser, new List<string> { Roles.RoleAdmin });

        // Act
        var response = await client.PatchAsync($"api/users/{blockedUser.Id}/unblock", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Неверный статус код ответа");

        // Verify user is unblocked
        var unblockedUser = await DbContext.Users
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == blockedUser.Id);
            
        Assert.IsFalse(unblockedUser.IsBlocked, "Пользователь должен быть разблокирован");
        
        // Verify that ChangeHistoryCreateByUserIdRequest was published
        bool sent = await Harness.Published.Any<ChangeHistoryCreateByUserIdRequest>();
        Assert.IsTrue(sent, "Событие ChangeHistoryCreateByUserIdRequest не было отправлено.");
    }

    [Test]
    public async Task UnblockUser_ShouldReturnForbidden_WhenNotAdmin()
    {
        // Arrange
        var regularUser = UserFactory.Default(_passwordHasher, DbContext);
        var blockedUser = UserFactory.Default(_passwordHasher, DbContext);
        blockedUser.IsBlocked = true;
        await DbContext.SaveChangesAsync();
        
        var client = CreateAuthenticatedClient(regularUser);

        // Act
        var response = await client.PatchAsync($"api/users/{blockedUser.Id}/unblock", null);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "Должна вернуться 403 для пользователя без прав администратора");
    }

    [Test]
    public async Task UnblockUser_ShouldReturnNotFound_WhenUserNotExists()
    {
        // Arrange
        var adminUser = UserFactory.Default(_passwordHasher, DbContext);
        await _userManager.AddToRoleAsync(adminUser, Roles.RoleAdmin);
        var client = CreateAuthenticatedClient(adminUser, new List<string> { Roles.RoleAdmin });
        var nonExistentUserId = Guid.NewGuid().ToString();

        // Act
        var response = await client.PatchAsync($"api/users/{nonExistentUserId}/unblock", null);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Должна вернуться 404 для несуществующего пользователя");
    }
}
