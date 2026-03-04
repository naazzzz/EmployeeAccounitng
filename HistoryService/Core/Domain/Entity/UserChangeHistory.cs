using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using General.ValueObjects.Enums;

namespace HistoryService.Core.Domain.Entity;

public class UserChangeHistory
{
    [Key] [StringLength(50)] public string Id { get; set; } = Guid.NewGuid().ToString();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public HistoryActionEnum ActionEnum { get; set; }

    [Column(TypeName = "jsonb")] public required string ValueAction { get; set; }

    [ForeignKey("User")]
    [StringLength(50)]
    public required string UserId { get; set; }
}