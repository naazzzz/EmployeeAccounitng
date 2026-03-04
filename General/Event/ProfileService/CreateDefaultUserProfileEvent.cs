namespace General.Event.ProfileService;

public class CreateDefaultUserProfileEvent
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
}