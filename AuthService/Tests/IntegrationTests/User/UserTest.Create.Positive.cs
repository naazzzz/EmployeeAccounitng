using System.Text;
using AuthService.Web.Dto.User;
using General.Event.ChangeHistoryService;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;


namespace AuthService.Tests.IntegrationTests.User;

[TestFixture]
[TestOf(typeof(UserTest))]
public partial class UserTest : BaseTest
{
    protected override void ConfigureMassTransit(IBusRegistrationConfigurator x)
    {
        x.UsingInMemory((context, cfg) => { cfg.ConfigureEndpoints(context); });

        x.AddRequestClient<ChangeHistoryRequest>();
        x.AddRequestClient<ChangeHistoryCreateByUserIdRequest>();
        x.AddRequestClient<ChangeHistoryRequestLatestByUserId>();
    }


    [Test]
    public async Task Should_create_user_success()
    {
        // Arrange: Подготовка данных для запроса
        var newUser = new
        {
            UserName = "testuser",
            Email = "testuser@example.com",
            PlainPassword = "Test123!",
            PhoneNumber = "+79000000000"
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(newUser),
            Encoding.UTF8,
            "application/json");

        // Act: Выполняем POST-запрос к эндпоинту
        var response = await Client.PostAsync("/api/users", content);

        // Assert: Проверяем статус ответа
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode, "Неверный статус код ответа");

        // Десериализуем и проверяем тело ответа
        var userResponseDto = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.IsNotNull(userResponseDto, "Не удалось десериализовать ответ сервера");
        
        // Проверяем, что ID в ответе корректный
        Assert.IsFalse(string.IsNullOrEmpty(userResponseDto.Id), "ID пользователя не должен быть пустым");
        Assert.IsTrue(Guid.TryParse(userResponseDto.Id, out _), "ID пользователя должен быть в формате GUID");
        
        // Проверяем соответствие полей запроса и ответа
        Assert.AreEqual(newUser.UserName, userResponseDto.UserName, "Имя пользователя не совпадает");
        Assert.AreEqual(newUser.Email, userResponseDto.Email, "Email не совпадает");
        Assert.AreEqual(newUser.PhoneNumber, userResponseDto.PhoneNumber, "Номер телефона не совпадает");
        
        // Проверяем, что пароль не возвращается в ответе
        Assert.IsNull(userResponseDto.PasswordHash, "Пароль не должен возвращаться в ответе");
        
        // Проверяем, что пользователь действительно сохранен в БД
        var savedUser = await DbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userResponseDto.Id);

        Assert.IsNotNull(savedUser, "Пользователь не был добавлен в базу данных");
        Assert.AreEqual(newUser.UserName, savedUser.UserName, "Имя пользователя в БД не совпадает");
        Assert.AreEqual(newUser.Email, savedUser.Email, "Email в БД не совпадает");
        Assert.AreEqual(newUser.PhoneNumber, savedUser.PhoneNumber, "Номер телефона в БД не совпадает");
        
        // Проверяем, что пароль захеширован в БД
        Assert.IsNotNull(savedUser.PasswordHash, "Пароль должен быть захеширован в БД");
        Assert.AreNotEqual(newUser.PlainPassword, savedUser.PasswordHash, "Пароль не должен храниться в открытом виде");
    }
}