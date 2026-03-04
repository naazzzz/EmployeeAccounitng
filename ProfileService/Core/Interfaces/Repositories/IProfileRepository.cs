using General.Interfaces;
using ProfileService.Core.Domain.Entities;

namespace ProfileService.Core.Interfaces.Repositories;

public interface IProfileRepository : IRepository<Profile>
{
    Task UpdateAsync(Profile entityWithNewValue);

    Task<Profile?> GetByIdAsyncNoTracking(string userId);

    Task<Profile?> GetByNameAsync(string username);

    Task<Profile?> GetByEmailAsync(string email);

    void LoadActualDepartment(Profile profile);
}