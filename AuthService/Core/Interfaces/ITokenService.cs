namespace AuthService.Core.Interfaces;

public interface ITokenService
{
    Task<string> Authorize(string username, string password);
}