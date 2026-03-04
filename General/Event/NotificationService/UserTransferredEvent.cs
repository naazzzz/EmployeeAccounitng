namespace General.Event.NotificationService;

public class UserTransferredEvent
{
    public string DepartmentName {get; set;}

    public string UserName { get; set; }
    
    public string Email { get; set; }
}