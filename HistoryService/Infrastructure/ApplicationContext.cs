using HistoryService.Core.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace HistoryService.Infrastructure;

public class ApplicationContext: DbContext
{
    public DbSet<UserChangeHistory> UserChangeHistory { get; set; }
    
    public DbSet<ChangeHistory> ChangeHistory { get; set; }
    
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }
}