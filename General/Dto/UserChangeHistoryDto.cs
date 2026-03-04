using General.ValueObjects.Enums;

namespace General.Dto;

public class UserChangeHistoryDto
{
    public string Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public HistoryActionEnum ActionEnum { get; set; }
    
    public required string ValueAction { get; set; }
    
    public required string UserId { get; set; }
    
    public bool NeedCode { get; set; }
}