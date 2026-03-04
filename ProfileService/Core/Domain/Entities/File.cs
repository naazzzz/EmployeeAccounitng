using System.ComponentModel.DataAnnotations;

namespace ProfileService.Core.Domain.Entities;

public class File
{
    [Key] [StringLength(255)] public string Id { get; set; } = Guid.NewGuid().ToString();

    public required string Name { get; set; }
    public required string Path { get; set; }
}