
using AuthService.Core.Entities;
using General.Interfaces;

namespace AuthService.Core.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task UpdateAsync(User entityWithNewValue);

    Task<User?> GetByIdAsyncNoTracking(string userId);

    Task<User?> GetByNameAsync(string username);

    Task<User?> GetByEmailAsync(string email, bool includeFilter = false);

    Task<List<User>> GetAllUsersWithExpiresPassword();
}