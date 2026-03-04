using AuthService.Core.Entities;
using AuthService.Infrastructure.DbContext;
using Bogus;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Tests.Factories;

public static class UserFactory
{
    public static string DefaultPassword = "Secret000";
    public static User Default(IPasswordHasher<User> passwordHasher)
    {
        return new Faker<User>()
            .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
            .RuleFor(u => u.CreatedAt, f => f.Date.PastOffset(1, DateTimeOffset.UtcNow))
            .RuleFor(u => u.Email, f => f.Person.Email)
            .RuleFor(u => u.UserName, f => f.Person.UserName)
            .RuleFor(u => u.PasswordHash, f => passwordHasher.HashPassword(new User(), DefaultPassword))
            .RuleFor(u => u.EmailConfirmed, f => true)
            .RuleFor(u => u.TwoFactorEnabled, f => false)
            .RuleFor(u => u.PhoneNumber, f => f.Person.Phone);
    }
    
    public static User Default(IPasswordHasher<User> passwordHasher, ApplicationContext context)
    {
        var user = Default(passwordHasher);
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }
}