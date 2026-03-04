using AuthService.Core.Entities;
using General.Interfaces;

namespace AuthService.Core.Interfaces;

public interface IСonfirmationCodeRepository: IRepository<ConfirmationCode>
{
    ConfirmationCode? GetMailCodeByCodeAndUserId(string code, string userId);
    
    ConfirmationCode? GetMailCodeByRefreshCodeAndUserId(string code, string userId);

    int RemoveExpiredCodes();
}