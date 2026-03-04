using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AuthService.Core.Events;

public class SavingChangesEventArgs: EventArgs
{
    public IEnumerable<EntityEntry> Entries { get; }
    public string CurrentUser { get; }
    
    public SavingChangesEventArgs(IEnumerable<EntityEntry> entries, string currentUser)
    {
        Entries = entries;
        CurrentUser = currentUser;
    }
}