using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using General.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProfileService.Core.Domain.ValueObjects.Enums;

namespace ProfileService.Core.Domain.Entities;

[Index(nameof(UserName), nameof(Email), IsUnique = true)]
public class Profile: IAuditable
{
    [Key] [StringLength(255)] public string Id { get; set; } = Guid.NewGuid().ToString();

    [StringLength(195)]
    public string? FirstName { get; set; }
    
    [StringLength(195)]
    public string? SecondName { get; set; }    
    
    [StringLength(195)]
    public string? FullName { get; set; }
    
    [Required] [StringLength(195)] public string? UserName { get; set; }

    [Required] [StringLength(195)] public string? Email { get; set; }

    [StringLength(50)] public string? PhoneNumber { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; set; }
    
    public string UpdatedBy { get; set; }

    [Column(TypeName = "jsonb")] public Address? Address { get; set; }

    public UsersGrade UsersGrade { get; set; } = UsersGrade.Default;

    public Department? Department { get; set; }

    [ForeignKey("Department")]
    [StringLength(195)]
    [Required]
    public string? DepartmentId { get; set; }
    
    [StringLength(195)]
    public required string UserId { get; set; }

    // todo пока так, но в будущем я бы переместил это в контекст
    [NotMapped] public bool IsChangeConfirmed { get; set; }

    public Avatar? Avatar { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public bool IsDeleted { get; set; }

    public Profile()
    {
    }

    public Profile(string username, Address? address = null, string? email = null, string? phone = null)
    {
        UserName = username;
        Address = address;
        Email = email;
        PhoneNumber = phone;
    }
    
    public Profile CloneShallow()
    {
        return (Profile)MemberwiseClone();
    }
}