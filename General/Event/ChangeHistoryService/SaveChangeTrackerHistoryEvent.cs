using General.Dto;

namespace General.Event.ChangeHistoryService;

public class SaveChangeTrackerHistoryEvent
{
    public SaveChangeTrackerHistoryEvent(ChangeHistoryDto changeHistoryDto)
    {
        ChangeHistoryDto = changeHistoryDto;
    }

    public ChangeHistoryDto ChangeHistoryDto { get; set; }
}