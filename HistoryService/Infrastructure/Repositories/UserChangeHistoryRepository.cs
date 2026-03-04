using General.Base;
using HistoryService.Core.Domain.Entity;
using HistoryService.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HistoryService.Infrastructure.Repositories;

public class UserChangeHistoryRepository(ApplicationContext context)
    : BaseRepository<UserChangeHistory>(context), IUserChangeHistoryRepository
{
    private readonly ApplicationContext _context = context;
    
    public async Task<UserChangeHistory?> GetLastChanges(string userId)
    {
        return await _context.UserChangeHistory.
            Where(u => u.UserId == userId).
            OrderByDescending(u => u.CreatedAt).
            FirstAsync();
    }
}