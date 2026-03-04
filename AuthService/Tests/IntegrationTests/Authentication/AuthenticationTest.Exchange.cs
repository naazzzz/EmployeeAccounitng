using System.Net;
using AuthService.Tests.Factories;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using OpenIddict.Abstractions;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace AuthService.Tests.IntegrationTests.Authentication;

[TestFixture]
[TestOf(typeof(AuthenticationTest))]
public partial class AuthenticationTest : BaseTest
{
    private const string TokenEndpoint = "/connect/token";
    private const string TestPassword = "Test123!";

    private IPasswordHasher<Core.Entities.User> _passwordHasher;

    [SetUp]
    public void Setup()
    {
        _passwordHasher = Scope.ServiceProvider.GetRequiredService<IPasswordHasher<Core.Entities.User>>();
    }
    
    [Test]
    public async Task Exchange_WithValidCredentials_ReturnsTokenResponse()
    {
        UserFactory.DefaultPassword = TestPassword;
        var user = UserFactory.Default(_passwordHasher, DbContext);
        
        var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [OpenIddictConstants.Parameters.GrantType] = OpenIddictConstants.GrantTypes.Password,
                [OpenIddictConstants.Parameters.Username] = user.UserName!,
                [OpenIddictConstants.Parameters.Password] = TestPassword,
                [OpenIddictConstants.Parameters.ClientId] = "profile_client",
                [OpenIddictConstants.Parameters.ClientSecret] = "secret",
            })
        };

        var response = await Client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"Expected OK but got {response.StatusCode}. Response: {content}");
        Assert.IsTrue(content.Contains("access_token"), "Response should contain access_token");
        Assert.IsTrue(content.Contains("token_type"), "Response should contain token_type");
        Assert.IsTrue(content.Contains("expires_in"), "Response should contain expires_in");
    }

    [Test]
    public async Task Exchange_WithInvalidCredentials_ReturnsBadRequest()
    {
        UserFactory.DefaultPassword = TestPassword;
        var user = UserFactory.Default(_passwordHasher, DbContext);
        
        var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [OpenIddictConstants.Parameters.GrantType] = OpenIddictConstants.GrantTypes.Password,
                [OpenIddictConstants.Parameters.Username] = user.UserName!,
                [OpenIddictConstants.Parameters.Password] = "wrong_password",
                [OpenIddictConstants.Parameters.ClientId] = "profile_client",
                [OpenIddictConstants.Parameters.ClientSecret] = "secret",
            })
        };

        // Act
        var response = await Client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, $"Expected BadRequest but got {response.StatusCode}");
        Assert.IsTrue(content.Contains("invalid_grant"), "Response should contain invalid_grant error");
    }
}
