using System.Text.Json.Serialization;

namespace AuthService.Web.Dto.User;

public class UserResponseDto : Core.Entities.User
{
    [JsonIgnore] public override string? PasswordHash { get; set; }

    [JsonIgnore] public new string? PlainPassword { get; set; }
}