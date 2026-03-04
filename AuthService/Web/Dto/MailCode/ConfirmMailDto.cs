namespace AuthService.Web.Dto.MailCode;

public class ConfirmMailDto(string code, string userId)
{
    public string Code { get; set; } = code;

    public string UserId { get; set; } = userId;
}