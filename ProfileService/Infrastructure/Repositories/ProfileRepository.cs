using General.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProfileService.Core.Domain.Entities;
using ProfileService.Core.Interfaces.Repositories;
using ApplicationContext = ProfileService.Infrastructure.DbContext.ApplicationContext;

namespace ProfileService.Infrastructure.Repositories;

public sealed class ProfileRepository(
    ApplicationContext context,
    IServiceProvider serviceProvider
    ) : BaseRepository<Profile>(context), IProfileRepository
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ApplicationContext _context = context;

    public async Task UpdateAsync(Profile entityWithNewValue)
    {
        _context.Profiles.Update(entityWithNewValue);
        await Context.SaveChangesAsync();
    }

    public async Task<Profile?> GetByIdAsyncNoTracking(string userId)
    {
        return await _context.Profiles.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<Profile?> GetByNameAsync(string username)
    {
        return await _context.Profiles.FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<Profile?> GetByEmailAsync(string email)
    {
        return await _context.Profiles.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async void LoadActualDepartment(Profile profile)
    {
        await Context.Entry(profile).Reference(a => a.Department).LoadAsync();
    }
}