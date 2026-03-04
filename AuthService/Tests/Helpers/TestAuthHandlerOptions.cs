namespace AuthService.Tests.Helpers;

public class TestAuthHandlerOptions
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = new List<string>{ General.Auth.Roles.RoleUser };
}
