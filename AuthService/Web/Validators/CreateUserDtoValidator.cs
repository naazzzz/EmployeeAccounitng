using System.ComponentModel.DataAnnotations;
using AuthService.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Validators;

public class UniqueAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var context = validationContext.GetRequiredService<ApplicationContext>();
        if (context == null)
        {
            return new ValidationResult("ApplicationContext не зарегистрирован в контейнере зависимостей.");
        }
        var fieldName = validationContext.DisplayName;

        var field = value as string;
        if (field == null) return new ValidationResult($"{fieldName} не может быть null.");

        foreach (var user in context.Users.IgnoreQueryFilters())
        {
            var propertyInfo = user.GetType().GetProperty(fieldName);
            if (propertyInfo == null) return new ValidationResult($"Свойство {fieldName} не найдено.");

            var propertyValue = propertyInfo.GetValue(user) as string;
            if (propertyValue == field) return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}