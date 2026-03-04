using General.Interfaces;
using HistoryService.Core.Domain.Entity;

namespace HistoryService.Core.Interfaces;

public interface IUserChangeHistoryRepository : IRepository<UserChangeHistory>
{
    Task<UserChangeHistory?> GetLastChanges(string userId);
}