using System.Net;
using AuthService.Tests.Factories;
using AuthService.Web.Dto.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace AuthService.Tests.IntegrationTests.User;

public partial class UserTest
{
    [Test]
    public async Task Should_get_item_user_success()
    {
        var user = UserFactory.Default(
            Scope.ServiceProvider.GetRequiredService<IPasswordHasher<Core.Entities.User>>(),
            DbContext);
        var client = CreateAuthenticatedClient(user);

        var response = await client.GetAsync($"api/users/{user.Id}");

        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Неверный статус код ответа");

        var userResponseDto = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.IsNotNull(userResponseDto, "Не удалось десериализовать ответ сервера");

        Assert.IsFalse(string.IsNullOrEmpty(userResponseDto.Id), "ID пользователя не должен быть пустым");
        Assert.IsTrue(Guid.TryParse(userResponseDto.Id, out _), "ID пользователя должен быть в формате GUID");

        Assert.AreEqual(user.UserName, userResponseDto.UserName, "Имя пользователя не совпадает");
        Assert.AreEqual(user.Email, userResponseDto.Email, "Email не совпадает");
        Assert.AreEqual(user.PhoneNumber, userResponseDto.PhoneNumber, "Номер телефона не совпадает");

        Assert.IsNull(userResponseDto.PasswordHash, "Пароль не должен возвращаться в ответе");
    }
    
    [Test]
    public async Task Should_get_collection_user_success_role_user()
    {
        var user = UserFactory.Default(
            Scope.ServiceProvider.GetRequiredService<IPasswordHasher<Core.Entities.User>>(),
            DbContext);

        var otherUser = UserFactory.Default(
            Scope.ServiceProvider.GetRequiredService<IPasswordHasher<Core.Entities.User>>(),
            DbContext);
        
        var client = CreateAuthenticatedClient(user);

        var response = await client.GetAsync($"api/users");

        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Неверный статус код ответа");

        var userResponseDto = await response.Content.ReadFromJsonAsync<List<UserResponseDto>>();
        Assert.IsNotNull(userResponseDto, "Не удалось десериализовать ответ сервера");

        Assert.AreEqual(userResponseDto.Count, 1, "Количество пользователей не совпадает");
        
        Assert.IsFalse(string.IsNullOrEmpty(userResponseDto[0].Id), "ID пользователя не должен быть пустым");
        Assert.IsTrue(Guid.TryParse(userResponseDto[0].Id, out _), "ID пользователя должен быть в формате GUID");

        Assert.AreEqual(user.UserName, userResponseDto[0].UserName, "Имя пользователя не совпадает");
        Assert.AreEqual(user.Email, userResponseDto[0].Email, "Email не совпадает");
        Assert.AreEqual(user.PhoneNumber, userResponseDto[0].PhoneNumber, "Номер телефона не совпадает");

        Assert.IsNull(userResponseDto[0].PasswordHash, "Пароль не должен возвращаться в ответе");
    }

    [Test]
    public async Task Should_get_collection_user_success_role_admin()
    {
        var user = UserFactory.Default(
            Scope.ServiceProvider.GetRequiredService<IPasswordHasher<Core.Entities.User>>(),
            DbContext);

        var otherUser = UserFactory.Default(
            Scope.ServiceProvider.GetRequiredService<IPasswordHasher<Core.Entities.User>>(),
            DbContext);

        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<Core.Entities.User>>();
        await userManager.AddToRoleAsync(user, General.Auth.Roles.RoleAdmin);

        var client = CreateAuthenticatedClient(user, new List<string>{General.Auth.Roles.RoleAdmin});

        var response = await client.GetAsync($"api/users");

        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Неверный статус код ответа");

        var userResponseDto = await response.Content.ReadFromJsonAsync<List<UserResponseDto>>();
        Assert.IsNotNull(userResponseDto, "Не удалось десериализовать ответ сервера");

        Assert.AreEqual(userResponseDto.Count, 4, "Количество пользователей не совпадает");
    }
}