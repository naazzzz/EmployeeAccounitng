using General.Exceptions;
using ProfileService.Core.Domain.Entities;
using ProfileService.Core.Interfaces.Repositories;
using ProfileService.Core.Interfaces.Services;

namespace ProfileService.Core.Services;

public class AvatarService : IAvatarService
{
    private readonly IAvatarRepository _avatarRepository;

    public AvatarService(IAvatarRepository avatarRepository)
    {
        _avatarRepository = avatarRepository;
    }

    public async Task<Avatar?> Create(string profileId, string fileId)
    {
        var avatar = new Avatar
        {
            ProfileId = profileId,
            FileId = fileId
        };

        return await _avatarRepository.AddWithSaveAsync(avatar);
    }

    public async Task<Avatar?> ChangeFile(string avatarId, string fileId)
    {
        var avatar = await _avatarRepository.GetByIdAsync(avatarId);
        if (avatar == null) throw new RecordNotFoundException(avatarId, "Avatar");

        avatar.FileId = fileId;
        await _avatarRepository.SaveChanges();
        _avatarRepository.LoadActualFile(avatar);

        return avatar;
    }

    public async Task<List<Avatar>> GetCollection()
    {
        var avatars = await _avatarRepository.GetAllAsync();
        if (avatars.Count == 0) throw new RecordNotFoundException("Collection", "Avatars");

        return avatars;
    }

    public async Task<Avatar?> GetItem(string avatarId)
    {
        var avatar = await _avatarRepository.GetByIdAsync(avatarId);
        if (avatar == null) throw new RecordNotFoundException(avatarId, "Avatar");

        return avatar;
    }

    public async Task Delete(string avatarId)
    {
        await _avatarRepository.DeleteWithSaveAsync(avatarId);
    }
}