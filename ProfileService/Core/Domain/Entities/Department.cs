using System.ComponentModel.DataAnnotations;

namespace ProfileService.Core.Domain.Entities;

public class Department
{
    [Key] [StringLength(255)] public string Id { get; set; } = Guid.NewGuid().ToString();

    public DateTimeOffset CreateAt { get; set; } = DateTimeOffset.UtcNow;

    [StringLength(255)] public required string Name { get; set; }
}