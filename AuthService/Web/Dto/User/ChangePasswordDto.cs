using System.ComponentModel.DataAnnotations;

namespace AuthService.Web.Dto.User;

public class ChangePasswordDto(string newPassword, string oldPassword)
{
    [Required(ErrorMessage = "Пароль обязателен.")]
    [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Пароль должен содержать как минимум одну заглавную букву, одну строчную букву и одну цифру.")]
    public string NewPassword { get; set; } = newPassword;
    
    [Required(ErrorMessage = "Пароль обязателен.")]
    [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Пароль должен содержать как минимум одну заглавную букву, одну строчную букву и одну цифру.")]
    public string OldPassword { get; set; } = oldPassword;
}