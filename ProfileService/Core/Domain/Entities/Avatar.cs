using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProfileService.Core.Domain.Entities;

public class Avatar
{
    [Key] [StringLength(255)] public string Id { get; set; } = Guid.NewGuid().ToString();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public required string FileId { get; set; }
    
    public File? File { get; set; }

    public required string ProfileId { get; set; }

    [JsonIgnore]
    public Profile? Profile { get; set; }
}