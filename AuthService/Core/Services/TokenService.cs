using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using AuthService.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace AuthService.Core.Services;

public class TokenService: ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<Entities.User> _userManager;
    private readonly IPasswordHasher<Entities.User> _passwordHasher;
    
    public TokenService(IConfiguration configuration, UserManager<Entities.User> userManager, IPasswordHasher<Entities.User> passwordHasher)
    {
        _configuration = configuration;
        _userManager = userManager;
        _passwordHasher = passwordHasher;
    }

    public async Task<string> Authorize(string username, string password)
    {
        var user = await _userManager.Users.Where(u => u.UserName == username).FirstOrDefaultAsync();
        
        // if (user != null && user.CanLogin() &&
        if (user != null &&
        (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, password) == PasswordVerificationResult.Success ||
         _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, password) == PasswordVerificationResult.SuccessRehashNeeded))
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            return GenerateJwtToken(user, roles);
        }
        
        throw new InvalidCredentialException();
    }

    private  string GenerateJwtToken(IdentityUser user, IList<string>? roles = null)
    {
        
        var claims = new List<Claim>()
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Aud, _configuration["JWT_AUD_ISS_CLAIM"] ?? string.Empty),
            new(JwtRegisteredClaimNames.Iss, _configuration["JWT_AUD_ISS_CLAIM"] ?? string.Empty),
        };

        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var tokenSecret = _configuration["JWT_TOKEN_SECRET"];
        if (tokenSecret == null)
        {
            throw new Exception("token secret is null");
        }
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["JWT_AUD_ISS_CLAIM"] ?? string.Empty,
            audience: _configuration["JWT_AUD_ISS_CLAIM"] ?? string.Empty,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}