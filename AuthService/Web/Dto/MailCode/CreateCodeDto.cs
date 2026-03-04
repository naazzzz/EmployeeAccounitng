namespace AuthService.Web.Dto.MailCode;

public class CreateCodeDto(string userId, string? refreshCode)
{
    public string UserId { get; set; } = userId;
    public string? RefreshCode { get; set; } = refreshCode;
}