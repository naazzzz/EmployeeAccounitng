using System.Security.Claims;
using System.Text.Encodings.Web;
using General.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AuthService.Tests.Helpers;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string TestScheme = "Test";
    private readonly IOptions<TestAuthHandlerOptions> _options;
    
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IOptions<TestAuthHandlerOptions> testOptions) : base(options, logger, encoder, clock)
    {
        _options = testOptions;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = _options.Value.UserId ?? "00000000-0000-0000-0000-000000000001";
        var userEmail = _options.Value.UserEmail ?? "test@test.com";
        var username = _options.Value.UserName ?? "test";
        var roles = _options.Value.Roles ?? new[] { Roles.RoleUser };
        
        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, username),
            new (ClaimTypes.Email, userEmail),
            new (ClaimTypes.NameIdentifier, userId),
            new ("sub", userId),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, TestScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
