using Microsoft.AspNetCore.Identity;
using General.Exceptions;

namespace AuthService.Core.Services.User;

public sealed partial class UserService
{
    public async Task<Entities.User> ChangeEmail(string id, string email)
    {
        var user = await GetById(id);
        if (user == null) throw new RecordNotFoundException(id, "User");

        user.Email = email;
        user.EmailConfirmed = false;

        await _userRepository.SaveChanges();

        return user;
    }

    public async Task<Entities.User> ChangePassword(string id, string newPassword, string oldPassword)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) 
            throw new RecordNotFoundException(id, "User");

        var isOldPasswordValid = await _userManager.CheckPasswordAsync(user, oldPassword);
        if (!isOldPasswordValid)
            throw new UnauthorizedAccessException("Неверный текущий пароль");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
    
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Не удалось изменить пароль: {errors}");
        }

        return user;
    }

    public async Task<Entities.User> BlockUser(string id)
    {
        var user = await GetById(id);
        if (user == null) throw new RecordNotFoundException(id, "User");

        user.IsBlocked = true;

       await _userRepository.SaveChanges();

        return user;
    }

    public async Task<Entities.User> UnblockUser(string id)
    {
        var user = await GetById(id);
        if (user == null) throw new RecordNotFoundException(id, "User");


        user.IsBlocked = false;

        await _userRepository.SaveChanges();

        return user;
    }

    public PasswordVerificationResult CheckUserPassword(Entities.User profile, string password)
    {
        return _passwordHasher.VerifyHashedPassword(profile, profile.PasswordHash ?? string.Empty, password);
    }

    public async Task<Entities.User?> FindOrCreateForGoogleAuth(string email, string username)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {

            user = new Entities.User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
            };

            var createResult = await _userRepository.AddWithSaveAsync(user);
            if (createResult == null) return null;

            return createResult;
        }

        return user;
    }
}