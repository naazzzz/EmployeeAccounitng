using General.Base;
using Microsoft.EntityFrameworkCore;
using ProfileService.Core.Domain.Entities;
using ProfileService.Core.Interfaces.Repositories;
using ApplicationContext = ProfileService.Infrastructure.DbContext.ApplicationContext;

namespace ProfileService.Infrastructure.Repositories;

public class AvatarRepository(ApplicationContext context) : BaseRepository<Avatar>(context), IAvatarRepository
{
    public async void LoadActualFile(Avatar avatar)
    {
        await Context.Entry(avatar).Reference(a => a.File).LoadAsync();
    }
    
    public override async Task<Avatar?> GetByIdAsync(string id)
    {
        return await Context.Set<Avatar>().Include(a => a.File).Where(a => a.Id == id).FirstAsync();
    }

    public new async Task<List<Avatar>> GetAllAsync()
    {
        return await Context.Set<Avatar>().Include(a => a.File).ToListAsync();
    }
}