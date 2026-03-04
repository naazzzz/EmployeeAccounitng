namespace General.Event.ChangeHistoryService;

public record ChangeHistoryRequest(string ChangeHistoryId);

public record ChangeHistoryRequestLatestByUserId(string UserId);
public record ChangeHistoryCreateByUserIdRequest(Dto.UserChangeHistoryDto UserChangeHistoryDto);

public record ChangeHistoryResponse(Dto.UserChangeHistoryDto UserChangeHistoryDto);