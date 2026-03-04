using AuthService.Infrastructure.DbContext;
using General.Dto;
using General.Event.ChangeHistoryService;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Core.Events.Listeners;

public sealed class AuditListener : IDisposable
{
    private readonly ApplicationContext _applicationContext;
    private readonly ILogger<AuditListener> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuditListener(
        ApplicationContext applicationContext,
        ILogger<AuditListener> logger,
        IPublishEndpoint publishEndpoint)
    {
        _applicationContext = applicationContext;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _applicationContext.SavingChangesEvent += OnSavingChanges;
    }

    private void OnSavingChanges(object sender, SavingChangesEventArgs e)
    {
        var added = e.Entries.Where(entry => entry.State == EntityState.Added).ToList();
        var modified = e.Entries.Where(entry => entry.State == EntityState.Modified).ToList();
        var deleted = e.Entries.Where(entry => entry.State == EntityState.Deleted).ToList();

        _logger.LogInformation(
            $"Аудит от {e.CurrentUser}: Добавлено {added.Count}, Изменено {modified.Count}, Удалено {deleted.Count} сущностей.");

        foreach (var entry in e.Entries)
        {
            var changes = GetChanges(entry);

            string? entityId = null;
            
            // Try to get the ID property if it exists
            var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            
            if (idProperty != null)
            {
                entityId = entry.State == EntityState.Deleted
                    ? entry.OriginalValues["Id"]?.ToString()
                    : entry.Property("Id").CurrentValue?.ToString();
            }

            var action = entry.State switch
            {
                EntityState.Added => "Добавлена",
                EntityState.Modified => "Изменена",
                EntityState.Deleted => "Удалена",
                _ => "Неизвестное действие"
            };

            _logger.LogInformation($"{action} сущность {entry.Entity.GetType().Name} (ID: {entityId}): {changes}");

            if (entry.State != EntityState.Unchanged && entry.State != EntityState.Detached)
            {
                _publishEndpoint.Publish(new SaveChangeTrackerHistoryEvent(new ChangeHistoryDto
                {
                    CurrentUser = e.CurrentUser,
                    EntityName = entry.Entity.GetType().Name,
                    EntityId = entityId,
                    Changes = changes,
                    Action = action
                }));
            }
        }
    }

    private string GetChanges(EntityEntry entry)
    {
        return entry.State switch
        {
            EntityState.Added => string.Join("; ", entry.Properties
                .Where(p => p.CurrentValue != null)
                .Select(p => $"{p.Metadata.Name} = {p.CurrentValue}")),

            EntityState.Modified => string.Join("; ", entry.Properties
                .Where(p => p.IsModified)
                .Select(p => $"{p.Metadata.Name}: {p.OriginalValue} -> {p.CurrentValue}")),

            EntityState.Deleted => string.Join("; ", entry.Properties
                .Select(p => $"{p.Metadata.Name} = {p.OriginalValue}")),

            _ => "Нет изменений"
        };
    }

    public void Dispose()
    {
        _applicationContext.SavingChangesEvent -= OnSavingChanges;
    }
}