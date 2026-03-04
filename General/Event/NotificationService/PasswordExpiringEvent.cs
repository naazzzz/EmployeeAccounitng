namespace General.Event.NotificationService;

public class PasswordExpiringEvent
{
    public string UserName { get; set; }
    
    public string Email { get; set; }
}