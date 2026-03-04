using System.ComponentModel.DataAnnotations;

namespace AuthService.Web.Dto.User;

public sealed class UpdateUserDto(string username, string? phoneNumber)
{
    [Required(ErrorMessage = "Имя пользователя обязательно.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно содержать от 3 до 50 символов.")]
    [AuthService.Web.Validators.Unique(ErrorMessage = "Пользователь с таким именем уже существует.")]
    public string UserName { get; set; } = username;

    [RegularExpression(@"^((\+7|7|8)+([ ])?(\()?(\d{3})(\))?([ ])?\d{3}[- ]?\d{2}[- ]?\d{2})$",
        ErrorMessage = "Невалидный формат номера телефона.")]
    public string? PhoneNumber { get; set; } = phoneNumber;
}