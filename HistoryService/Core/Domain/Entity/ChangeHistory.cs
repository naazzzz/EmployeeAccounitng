using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HistoryService.Core.Domain.Entity;

public class ChangeHistory
{
    [Key] [StringLength(50)] public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public string CurrentUser { get; set; }
    
    public string EntityName { get; set; }
    
    public string? EntityId { get; set; }
    
    public string Changes { get; set; }
    
    public string Action { get; set; }
}