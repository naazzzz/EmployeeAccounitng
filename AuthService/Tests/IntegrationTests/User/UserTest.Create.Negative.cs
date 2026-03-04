using System.Net;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace AuthService.Tests.IntegrationTests.User;

public partial class UserTest
{
    [Test]
    public async Task CreateUser_WithNullPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var newUser = new
        {
            UserName = "testuser",
            Email = "testuser@example.com",
            PlainPassword = (string)null, // Explicit null password
            PhoneNumber = "+79000000000"
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(newUser),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await Client.PostAsync("/api/users", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Should return 400 Bad Request for null password");
    }

    [Test]
    public async Task CreateUser_WithExistingEmail_ShouldReturnConflict()
    {
        // Arrange - First create a user
        var existingUser = new
        {
            UserName = "existinguser",
            Email = "existing@example.com",
            PlainPassword = "Test123!",
            PhoneNumber = "+79000000001"
        };

        await CreateTestUser(existingUser);

        // Try to create another user with the same email
        var duplicateUser = new
        {
            UserName = "newuser",
            Email = existingUser.Email, // Same email
            PlainPassword = "Test123!",
            PhoneNumber = "+79000000002"
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(duplicateUser),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await Client.PostAsync("/api/users", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Should return 409 Conflict for duplicate email");
    }

    [Test]
    public async Task CreateUser_WithExistingUsername_ShouldReturnConflict()
    {
        // Arrange - First create a user
        var existingUser = new
        {
            UserName = "existingusername",
            Email = "user1@example.com",
            PlainPassword = "Test123!",
            PhoneNumber = "+79000000003"
        };

        await CreateTestUser(existingUser);

        // Try to create another user with the same username
        var duplicateUser = new
        {
            UserName = existingUser.UserName, // Same username
            Email = "user2@example.com",
            PlainPassword = "Test123!",
            PhoneNumber = "+79000000004"
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(duplicateUser),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await Client.PostAsync("/api/users", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Should return 409 Conflict for duplicate username");
    }

    [Test]
    public async Task CreateUser_WithInvalidEmailFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var newUser = new
        {
            UserName = "testuser",
            Email = "invalid-email-format", // Invalid email format
            PlainPassword = "Test123!",
            PhoneNumber = "+79000000005"
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(newUser),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await Client.PostAsync("/api/users", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Should return 400 Bad Request for invalid email format");
    }

    [Test]
    public async Task CreateUser_WithWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var newUser = new
        {
            UserName = "testuser",
            Email = "test@example.com",
            PlainPassword = "weak", // Too short password
            PhoneNumber = "+79000000006"
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(newUser),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await Client.PostAsync("/api/users", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Should return 400 Bad Request for weak password");
    }

    [Test]
    public async Task CreateUser_WithMissingRequiredFields_ShouldReturnBadRequest()
    {
        // Arrange - Missing username and email
        var newUser = new
        {
            // Missing UserName
            // Missing Email
            PlainPassword = "Test123!",
            PhoneNumber = "+79000000007"
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(newUser),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await Client.PostAsync("/api/users", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Should return 400 Bad Request for missing required fields");
    }

    [Test]
    public async Task CreateUser_WithInvalidPhoneNumber_ShouldReturnBadRequest()
    {
        // Arrange
        var newUser = new
        {
            UserName = "testuser",
            Email = "test@example.com",
            PlainPassword = "Test123!",
            PhoneNumber = "invalid-phone-number" // Invalid phone number format
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(newUser),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await Client.PostAsync("/api/users", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Should return 400 Bad Request for invalid phone number");
    }

    private async Task CreateTestUser(dynamic userData)
    {
        var content = new StringContent(
            JsonConvert.SerializeObject(userData),
            Encoding.UTF8,
            "application/json");

        var response = await Client.PostAsync("/api/users", content);
        response.EnsureSuccessStatusCode();
    }
}
