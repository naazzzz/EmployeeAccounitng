namespace General.Interfaces;

public interface IAuditable
{
    public DateTimeOffset CreatedAt { get; set; } 

    public DateTimeOffset? UpdatedAt { get; set; } 
    
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public void Undo()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}