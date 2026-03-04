namespace General.Event.AuthService;

public class CreateConfirmationCodeEvent
{
    public string UserId { get; set; }
    
    public string UserChangeHistoryId { get; set; }
}