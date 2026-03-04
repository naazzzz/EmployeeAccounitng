using System.ComponentModel.DataAnnotations;

namespace UserService.Web.Dto.User;

public class ChangeDepartment(string departmentId)
{
    [Required(ErrorMessage = "id департамента обязателен")]
    [RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$",
        ErrorMessage = "Некорректный формат GUID")]
    public string DepartmentId { get; set; } = departmentId;
}