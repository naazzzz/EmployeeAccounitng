using AuthService.Core.Entities;
using AuthService.Infrastructure.DbContext;
using Bogus;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Tests.Factories;

public static class CodeFactory
{
    public static ConfirmationCode Default(string userId, string changeHistoryId)
    {
        return new Faker<ConfirmationCode>()
            .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
            .RuleFor(u => u.Code, f => ConfirmationCode.RandomString(ConfirmationCode.CodeLength))
            .RuleFor(u => u.RefreshCode, f => ConfirmationCode.RandomString(ConfirmationCode.CodeLength))
            .RuleFor(u => u.ChangeHistoryId, f => changeHistoryId)
            .RuleFor(u => u.UserId, f => userId)
            .RuleFor(u => u.ExpiresAt, f => DateTimeOffset.UtcNow.AddHours(1));
    }
    
    public static ConfirmationCode Default(string userId, string changeHistoryId, ApplicationContext context)
    {
        var сonfirmationCode = Default(userId, changeHistoryId);
        context.ConfirmationCode.Add(сonfirmationCode);
        context.SaveChanges();
        return сonfirmationCode;
    }
}