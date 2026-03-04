using AuthService.Core.Entities;

namespace AuthService.Core.Interfaces;

public interface IСonfirmationCodeService
{
    Task FindAndAcceptCode(string code, string userId);

    Task CreateNewCodeForUserByRefreshCode(string userId, string refreshCode);
    
    Task<dynamic> CreateNewCode(string userId, string userChangeHistoryId);

    void RemoveExpiredCodes();
}