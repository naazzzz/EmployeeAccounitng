using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ISoftDelete = AuthService.Core.Interfaces.ISoftDelete;

namespace AuthService.Infrastructure.Interceptors;

public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<SoftDeleteInterceptor> _logger;

    public SoftDeleteInterceptor(ILogger<SoftDeleteInterceptor> logger)
    {
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Saving changes to the database at {Time}", DateTime.UtcNow);

        if (eventData.Context is null) return new ValueTask<InterceptionResult<int>>(result);

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            Console.WriteLine(entry.Entity);
            if (entry is not { State: EntityState.Deleted, Entity: ISoftDelete delete }) continue;

            entry.State = EntityState.Modified;
            delete.IsDeleted = true;
            delete.DeletedAt = DateTimeOffset.UtcNow;
        }

        return new ValueTask<InterceptionResult<int>>(result);
    }
}