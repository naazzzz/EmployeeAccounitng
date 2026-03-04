using Microsoft.AspNetCore.Identity;
using AuthService.Core.Entities;
using AuthService.Core.Interfaces.Repositories;
using AuthService.Infrastructure.DbContext;
using General.Auth;
using General.Base;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public sealed class UserRepository(
    ApplicationContext context,
    RoleManager<IdentityRole> roleManager,
    UserManager<User> userManager,
    IServiceProvider serviceProvider) : BaseRepository<User>(context), IUserRepository
{
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ApplicationContext _context = context;

    public async Task UpdateAsync(User entityWithNewValue)
    {
        _context.Users.Update(entityWithNewValue);
        await Context.SaveChangesAsync();
    }

    public async Task<User?> GetByIdAsyncNoTracking(string userId)
    {
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByNameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<User?> GetByEmailAsync(string email, bool includeFilter = false)
    {
        if (includeFilter)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        
        //todo пересмотреть, как будто бы полукостыль
        return await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email);
    }

    public override async Task<User?> AddWithSaveAsync(User entity)
    {
        entity.NormalizedUserName = _userManager.NormalizeName(entity.UserName);
        entity.NormalizedEmail = _userManager.NormalizeEmail(entity.Email);

        var result = _context.Users.AddAsync(entity);
        if (!result.IsCompletedSuccessfully) throw new Exception("Failed to add user");

        var role = await _roleManager.FindByNameAsync(Roles.RoleUser);
        if (role == null)
        {
            role = new IdentityRole(Roles.RoleUser);
            await _roleManager.CreateAsync(role); 
        }

        _context.UserRoles.Add(new IdentityUserRole<string>
            { UserId = entity.Id, RoleId = role.Id }); 

        await _context.SaveChangesAsync();

        return entity;
    }

    public async Task<List<User>> GetAllUsersWithExpiresPassword()
    {
        return await _context.Users.Where(u => u.PasswordExpiresAt < DateTimeOffset.UtcNow).ToListAsync();
    }
}