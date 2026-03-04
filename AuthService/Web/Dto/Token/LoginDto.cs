using System.ComponentModel.DataAnnotations;

namespace Web.Dto.Account;

public class LoginDto(string username, string password)
{
    [Required(ErrorMessage = "Имя пользователя обязательно.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно содержать от 3 до 50 символов.")]
    public string UserName { get; set; } = username;
    
    [Required(ErrorMessage = "Пароль обязателен.")]
    [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов.")]
    public string Password { get; set; } = password;
}