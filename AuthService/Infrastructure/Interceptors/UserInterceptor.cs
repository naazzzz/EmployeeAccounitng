using System.Text.Json;
using AuthService.Core.Entities;
using AuthService.Infrastructure.DbContext;
using General.Dto;
using General.Event.AuthService;
using General.Event.ChangeHistoryService;
using General.Event.ProfileService;
using General.ValueObjects.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AuthService.Infrastructure.Interceptors;

public class UserInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<UserInterceptor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPublishEndpoint _publishEndpoint;

    public UserInterceptor(ILogger<UserInterceptor> logger, IServiceProvider serviceProvider, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _publishEndpoint = publishEndpoint;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null) return new ValueTask<InterceptionResult<int>>(result);

        var context = eventData.Context as ApplicationContext;
        if (context == null) return new ValueTask<InterceptionResult<int>>(result);

        var mailCodesToAdd = new List<(User User, UserChangeHistoryDto changeHistory)>();

        // todo потом удалить
        context.ChangeTracker.DetectChanges();
        Console.WriteLine(context.ChangeTracker.DebugView.LongView);

        foreach (var entry in eventData.Context.ChangeTracker.Entries<User>())
        {
            var user = entry.Entity;
            var change = new UserChangeHistoryDto
            {
                UserId = user.Id,
                ValueAction = JsonSerializer.Serialize(user),
                NeedCode = false
            };

            switch (true)
            {
                case var s when entry.State == EntityState.Added:
                    change.ActionEnum = HistoryActionEnum.ConfirmEmail;
                    change.NeedCode = true;

                    var userCopy = user.CloneShallow();
                    userCopy.EmailConfirmed = true;
                    change.ValueAction = JsonSerializer.Serialize(userCopy);

                    _ = PublishCreateUserProfileEvent(user.Id, user.UserName!, user.Email!, cancellationToken);
                    
                    mailCodesToAdd.Add((user, change));
                    break;

                case var s when entry.State == EntityState.Modified &&
                                (entry.Property(u => u.PasswordHash).IsModified ||
                                 entry.Property(u => u.Email).IsModified):
                    if (user.IsChangeConfirmed)
                    {
                        change.ActionEnum = HistoryActionEnum.ApplyChanges;
                        mailCodesToAdd.Add((user, change));
                        user.IsChangeConfirmed = false;
                    }
                    else
                    {
                        change.NeedCode = true;
                        change.ActionEnum = HistoryActionEnum.ChangeProfileDataRequest;
                        mailCodesToAdd.Add((user, change));

                        // откладываем применение текущих изменений
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                    }

                    break;

                case var s when entry.State == EntityState.Modified:
                    change.ActionEnum = HistoryActionEnum.ApplyChanges;
                    mailCodesToAdd.Add((user, change));
                    break;

                case var s when entry.State == EntityState.Unchanged:
                    break;
            }
        }

        SaveHistoryChanges(mailCodesToAdd, cancellationToken);

        return new ValueTask<InterceptionResult<int>>(result);
    }

    private void SaveHistoryChanges(
        List<(User user, UserChangeHistoryDto changeHistory)> mailCodesToAdd,
        CancellationToken cancellationToken = default)
    {
        foreach (var (user, changeHistory) in mailCodesToAdd)
        {
            _publishEndpoint.Publish(new ChangeHistoryCreateByUserIdRequest(changeHistory), cancellationToken);
            _logger.LogInformation("UserInterceptor: Sent message for User ChangeHistory {UserId}", user.Id);
        }
    }

    private async Task PublishCreateUserProfileEvent(string userId, string username, string email, CancellationToken cancellationToken = default)
    {
        await _publishEndpoint.Publish(new CreateDefaultUserProfileEvent
        {
            UserId = userId,
            UserName = username,
            Email = email
        }, cancellationToken);
        _logger.LogInformation("UserInterceptor: Sent message for User Create Profile {UserId}", userId);
    }
}