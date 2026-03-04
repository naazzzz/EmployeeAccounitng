using AuthService.Core.Interfaces.Repositories;
using AuthService.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Core.Services.User;

public sealed partial class UserService : IUserService
{
    private readonly IPasswordHasher<Entities.User> _passwordHasher;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<Entities.User> _userManager;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher<Entities.User> passwordHasher,
        UserManager<Entities.User> userManager)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _userManager = userManager;
    }

    public Task<List<Entities.User>> GetAll()
    {
        return _userRepository.GetAllAsync();
    }

    public Task<Entities.User?> GetById(string id)
    {
        return _userRepository.GetByIdAsync(id);
    }

    public async Task<Entities.User?> Create(Entities.User user)
    {
        if (user.PlainPassword != null)
            user.PasswordHash = _passwordHasher.HashPassword(user, user.PlainPassword);
        else
            //todo make custom error
            throw new Exception("User's plain password is null");

        var taskUser = await _userRepository.AddWithSaveAsync(user);
        return taskUser;
    }

    public Task SoftDelete(string id)
    {
        return _userRepository.DeleteWithSaveAsync(id);
    }
}