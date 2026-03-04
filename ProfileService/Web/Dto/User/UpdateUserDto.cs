using System.ComponentModel.DataAnnotations;
using ProfileService.Core.Domain.Entities;
using ProfileService.Web.Validators;

namespace ProfileService.Web.Dto.User;

public sealed class UpdateProfileUserDto(string username, Address? address, string? phoneNumber, string? departmentId)
{
    [Required(ErrorMessage = "Имя пользователя обязательно.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно содержать от 3 до 50 символов.")]
    [Unique(ErrorMessage = "Пользователь с таким именем уже существует.")]
    public string UserName { get; set; } = username;

    public Address? Address { get; set; } = address;

    [RegularExpression(@"^((\+7|7|8)+([ ])?(\()?(\d{3})(\))?([ ])?\d{3}[- ]?\d{2}[- ]?\d{2})$",
        ErrorMessage = "Невалидный формат номера телефона.")]
    public string? PhoneNumber { get; set; } = phoneNumber;
    
    [Required(ErrorMessage = "id департамента обязателен")]
    [RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$",
        ErrorMessage = "Некорректный формат GUID")]
    public string? DepartmentId { get; set; } = departmentId;
}