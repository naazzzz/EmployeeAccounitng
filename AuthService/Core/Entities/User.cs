using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using General.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Core.Entities;

[Index(nameof(UserName), nameof(Email), IsUnique = true)]
public class User : IdentityUser, IAuditable
{
    public User()
    {
    }

    public User(string username, string? email = null, string? phone = null)
    {
        UserName = username;
        Email = email;
        PhoneNumber = phone;
    }
    
    [StringLength(195)] public override string? PasswordHash { get; set; }

    //todo 90 в конфиг вынести
    public DateTimeOffset PasswordExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddDays(90);

    [NotMapped] public string? PlainPassword { get; set; }

    [Required] [StringLength(195)] public sealed override string? UserName { get; set; }

    [Required] [StringLength(195)] public sealed override string? Email { get; set; }

    [StringLength(50)] public sealed override string? PhoneNumber { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public string CreatedBy { get; set; }
    
    public string UpdatedBy { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public bool IsDeleted { get; set; }
    
    public bool IsBlocked { get; set; }
    
    // todo пока так, но в будущем я бы переместил это в контекст
    [NotMapped] public bool IsChangeConfirmed { get; set; }
    
    public User CloneShallow()
    {
        return (User)MemberwiseClone();
    }
    
    public bool CanLogin()
    {
        return !(IsBlocked || IsDeleted);
    }
}