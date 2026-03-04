using General.Interfaces;
using ProfileService.Core.Domain.Entities;

namespace ProfileService.Core.Interfaces.Repositories;

public interface IAvatarRepository : IRepository<Avatar>
{
    void LoadActualFile(Avatar avatar);
}