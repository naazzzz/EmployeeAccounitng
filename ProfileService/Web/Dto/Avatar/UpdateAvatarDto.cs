using System.ComponentModel.DataAnnotations;

namespace ProfileService.Web.Dto.Avatar;

public class UpdateAvatarDto(string fileId)
{
    [Required(ErrorMessage = "id файла обязателен")]
    [RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$",
        ErrorMessage = "Некорректный формат GUID")]
    public string FileId { get; set; } = fileId;
}