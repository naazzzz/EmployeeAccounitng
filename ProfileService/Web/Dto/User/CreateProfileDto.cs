using System.ComponentModel.DataAnnotations;
using ProfileService.Core.Domain.Entities;
using ProfileService.Web.Validators;

namespace ProfileService.Web.Dto.User;

public sealed class CreateProfileDto(
    string username,
    string plainPassword,
    Address? address,
    string email,
    string? phoneNumber,
    string? departmentId
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

    public Address? Address { get; set; } = address;

    [Required(ErrorMessage = "Почта обязательна.")]
    [EmailAddress(ErrorMessage = "Невалидный формат почты")]
    [Unique(ErrorMessage = "Пользователь с такой почтой уже существует.")]
    public string Email { get; set; } = email;

    [RegularExpression(@"^((\+7|7|8)+([ ])?(\()?(\d{3})(\))?([ ])?\d{3}[- ]?\d{2}[- ]?\d{2})$",
        ErrorMessage = "Невалидный формат номера телефона.")]
    public string? PhoneNumber { get; set; } = phoneNumber;

    //todo лучше кастомным атрибутом с проверкой Guid.TryFrom
    [RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$",
        ErrorMessage = "Некорректный формат GUID")]
    public string? DepartmentId { get; set; } = departmentId;
}