using System.ComponentModel.DataAnnotations;
using AuthService.Web.Validators;

namespace AuthService.Web.Dto.User;

public sealed class CreateUserDto(
    string username,
    string plainPassword,
    string email,
    string? phoneNumber
)
{
    [Required(ErrorMessage = "Имя пользователя обязательно.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно содержать от 3 до 50 символов.")]
    [Unique(ErrorMessage = "Пользователь с таким именем уже существует.")]
    public string UserName { get; set; } = username;

    [Required(ErrorMessage = "Пароль обязателен.")]
    [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Пароль должен содержать как минимум одну заглавную букву, одну строчную букву и одну цифру.")]
    public string PlainPassword { get; set; } = plainPassword;

    [Required(ErrorMessage = "Почта обязательна.")]
    [EmailAddress(ErrorMessage = "Невалидный формат почты")]
    [Unique(ErrorMessage = "Пользователь с такой почтой уже существует.")]
    public string Email { get; set; } = email;

    [RegularExpression(@"^((\+7|7|8)+([ ])?(\()?(\d{3})(\))?([ ])?\d{3}[- ]?\d{2}[- ]?\d{2})$",
        ErrorMessage = "Невалидный формат номера телефона.")]
    public string? PhoneNumber { get; set; } = phoneNumber;
    
}