using AuthService.Core.Entities;
using AuthService.Infrastructure.DbContext;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Tests.Helpers;

public static class TestDataInitializer
{
    public static void InitializeTestData(this ApplicationContext context)
    {
        // Проверяем, есть ли уже роли
        if (!context.Roles.Any())
        {
            var roles = new[]
            {
                new IdentityRole 
                { 
                    Id = "a2011389-2df5-4a27-9e3c-1add9eb11d37", 
                    Name = General.Auth.Roles.RoleAdmin, 
                    NormalizedName = General.Auth.Roles.RoleAdmin.ToUpper() 
                },
                new IdentityRole 
                { 
                    Id = "d5408263-3a12-4812-bf5b-c12b7c500cb0", 
                    Name = General.Auth.Roles.RoleUser, 
                    NormalizedName = General.Auth.Roles.RoleUser.ToUpper() 
                }
            };
            
            context.Roles.AddRange(roles);
            context.SaveChanges();
        }

        // Проверяем, есть ли уже админ
        var adminId = "11111111-1111-1111-1111-111111111111";
        if (!context.Users.Any(u => u.Id == adminId))
        {
            var adminUser = new User
            {
                Id = adminId,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEERdUBT2upWECtSWExd5ger9eGVfUkzKqoRsS5npPBXtgn1ILZbYnEcmsCkov1t9Wg=="
            };

            context.Users.Add(adminUser);
            
            // Назначаем роль админа
            context.UserRoles.Add(new IdentityUserRole<string>
            {
                UserId = adminId,
                RoleId = "a2011389-2df5-4a27-9e3c-1add9eb11d37"
            });
            
            context.SaveChanges();
        }
    }
}