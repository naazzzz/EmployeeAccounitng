using System.ComponentModel.DataAnnotations;

namespace UserService.Web.Dto.User;

public class ChangeEmailDto(string email)
{
    [Required(ErrorMessage = "Почта обязательна.")]
    [EmailAddress(ErrorMessage = "Невалидный формат почты")]
    [AuthService.Web.Validators.Unique(ErrorMessage = "Пользователь с такой почтой уже существует.")]
    public string Email { get; set; } = email;
}