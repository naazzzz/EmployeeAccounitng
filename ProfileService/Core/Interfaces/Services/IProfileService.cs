using General.Interfaces;
using ProfileService.Core.Domain.Entities;

namespace ProfileService.Core.Interfaces.Services;

public interface IProfileService : IService<Profile>
{
    Task<List<Profile>> GetAll();

    Task<Profile?> GetById(string id);

    Task<Profile?> Create(Profile profile);

    Task<Profile?> UpdateProfileInfo(string id, Profile updateProfile);

    Task SoftDelete(string id);
}