using System.Text.Json;
using General.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProfileService.Core.Domain.Entities;
using File = ProfileService.Core.Domain.Entities.File;

namespace ProfileService.Infrastructure.DbContext;

public sealed class ApplicationContext : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly ICurrentUserService _currentUserService;
    
    public DbSet<Profile> Profiles { get; set; }
    
    public DbSet<File> Files { get; set; }
    
    public DbSet<Department> Departments { get; set; }

    public DbSet<Avatar> Avatars { get; set; }
    
    public ApplicationContext(DbContextOptions<ApplicationContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        var addressConverter = new ValueConverter<Address?, string>(
            v => v != null ? JsonSerializer.Serialize(v, new JsonSerializerOptions()) ?? string.Empty : string.Empty,
            v => v == null ? null : JsonSerializer.Deserialize<Address>(v, new JsonSerializerOptions()));

        modelBuilder.Entity<Profile>()
            .Property(u => u.Address)
            .HasConversion(addressConverter)
            .HasColumnName("Address");
        
        //todo вынести роли в общую библиотеку и использовать константы
        modelBuilder.Entity<Profile>()
            .HasQueryFilter(p => _currentUserService.IsInRole(General.Auth.Roles.RoleAdmin) || (p.IsDeleted == false &&
                _currentUserService.UserId != null && p.UserId == _currentUserService.UserId));
        
        modelBuilder.Entity<Avatar>()
            .HasQueryFilter(a => _currentUserService.IsInRole(General.Auth.Roles.RoleAdmin) || (
                a.Profile != null &&
                a.Profile.IsDeleted == false &&
                _currentUserService.UserId != null && 
                a.Profile.UserId == _currentUserService.UserId));
    }
    
}