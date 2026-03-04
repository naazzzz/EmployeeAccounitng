using AuthService.Core.Entities;
using AuthService.Core.Interfaces;
using AuthService.Infrastructure.DbContext;
using General.Base;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public sealed class СonfirmationCodeRepository(ApplicationContext context): BaseRepository<ConfirmationCode>(context), IСonfirmationCodeRepository
{
    private readonly ApplicationContext _context = context;
    
    public ConfirmationCode? GetMailCodeByCodeAndUserId(string code, string userId)
    {
        return _context.ConfirmationCode.FirstOrDefaultAsync(c =>
            c.ExpiresAt > DateTimeOffset.UtcNow &&
            c.Code == code.ToUpper() &&
            c.UserId == userId).Result;
    }

    public ConfirmationCode? GetMailCodeByRefreshCodeAndUserId(string code, string userId)
    {
        return _context.ConfirmationCode.FirstOrDefaultAsync(c =>
            c.ExpiresAt > DateTimeOffset.UtcNow &&
            c.RefreshCode == code.ToUpper() &&
            c.UserId == userId).Result;
    }

    public int RemoveExpiredCodes()
    {
       return _context.ConfirmationCode.Where(c => c.ExpiresAt < DateTimeOffset.UtcNow).ExecuteDelete();
    }
}