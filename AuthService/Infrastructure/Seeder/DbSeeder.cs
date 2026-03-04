using AuthService.Core.Entities;
using General.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace AuthService.Infrastructure.Seeder;

public class DatabaseSeeder
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<User> _userManager;

    public DatabaseSeeder(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        RoleManager<IdentityRole> roleManager,
        UserManager<User> userManager)
    {
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        if (await _applicationManager.FindByClientIdAsync("profile_client") == null)
        {
            await _applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "profile_client",
                ClientSecret = "secret",
                DisplayName = "Profile Service Client",
                RedirectUris = { new Uri("http://localhost:5001/signin-oidc") },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.GrantTypes.Password
                }
            });
        }

        if (await _scopeManager.FindByNameAsync("api1") == null)
        {
            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api1",
                DisplayName = "Profile API",
                Resources = { "http://localhost:5001/api/profiles" }
            });
        }
        
        if (await _roleManager.FindByNameAsync(Roles.RoleUser) == null)
        {
            await _roleManager.CreateAsync(
                new IdentityRole
                {
                    Name = Roles.RoleUser,
                    NormalizedName = Roles.RoleUser.ToUpper()
                }
            );
        }
        
        if (await _roleManager.FindByNameAsync(Roles.RoleAdmin) == null) 
        {
            await _roleManager.CreateAsync(
                new IdentityRole
                {
                    Name = Roles.RoleAdmin,
                    NormalizedName = Roles.RoleAdmin.ToUpper()
                }
            );
        }

        if (await _userManager.Users.IgnoreQueryFilters().Where(u => u.UserName == "admin").FirstOrDefaultAsync() == null)
        {
            var user = new User
            {
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEERdUBT2upWECtSWExd5ger9eGVfUkzKqoRsS5npPBXtgn1ILZbYnEcmsCkov1t9Wg=="
            };
            
            await _userManager.CreateAsync(user);
            
            await _userManager.AddToRoleAsync(
                user ?? throw new InvalidOperationException(),
                Roles.RoleAdmin);
        }
    }
}