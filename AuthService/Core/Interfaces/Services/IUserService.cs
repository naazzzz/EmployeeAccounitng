using AuthService.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Core.Interfaces.Services;

public interface IUserService : AuthService.Core.Interfaces.Services.IService<User>
{
    Task<List<User>> GetAll();

    Task<User?> GetById(string id);

    Task<User?> Create(User user);

    Task SoftDelete(string id);

    Task<User> ChangeEmail(string id, string email);

    Task<User> ChangePassword(string id, string newPassword, string oldPassword);

    Task<Entities.User> BlockUser(string id);

    Task<Entities.User> UnblockUser(string id);

    PasswordVerificationResult CheckUserPassword(User profile, string password);

    Task<User?> FindOrCreateForGoogleAuth(string email, string username);
    
}