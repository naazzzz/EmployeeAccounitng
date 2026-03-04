using ProfileService.Core.Domain.Entities;

namespace ProfileService.Core.Interfaces.Services;

public interface IAvatarService
{
    Task<Avatar?> Create(string profileId, string fileId);

    Task<Avatar?> ChangeFile(string avatarId, string fileId);

    Task<List<Avatar>> GetCollection();

    Task<Avatar?> GetItem(string avatarId);

    Task Delete(string avatarId);
}