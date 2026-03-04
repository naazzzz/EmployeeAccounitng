namespace General.Dto;

public class ChangeHistoryDto
{
    public string CurrentUser { get; set; }
    
    public string EntityName { get; set; }
    
    public string? EntityId { get; set; }
    
    public string Changes { get; set; }
    
    public string Action { get; set; }
}