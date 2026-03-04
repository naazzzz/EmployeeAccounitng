using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Core.Entities;

public class ConfirmationCode
{
    public const int CodeLength = 5;

    [Key]
    [StringLength(50)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(5)]
    public string Code { get; set; }
    
    [Required]
    [StringLength(5)]
    public string RefreshCode { get; set; }

    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddMinutes(5);

    [ForeignKey("User")]
    [Required]
    [StringLength(50)]
    public required string UserId { get; set; }

    [ForeignKey("UserChangeHistory")]
    [Required]
    [StringLength(50)]
    public required string ChangeHistoryId { get; set; }

    public ConfirmationCode()
    {
        Code = RandomString(CodeLength);
        RefreshCode = RandomString(CodeLength);
    }
    
    public ConfirmationCode(string userId, string changeHistoryId)
    {
        UserId = userId;
        ChangeHistoryId = changeHistoryId;
        Code = RandomString(CodeLength);
        RefreshCode = RandomString(CodeLength);
    }

    public static string RandomString(int length)
    {
        Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}