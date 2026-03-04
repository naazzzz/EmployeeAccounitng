using General.Dto;

namespace General.Event.AuthService;

public class CreateChangeHistoryEvent
{
    public UserChangeHistoryDto UserChangeHistoryDto { get; set; }
}