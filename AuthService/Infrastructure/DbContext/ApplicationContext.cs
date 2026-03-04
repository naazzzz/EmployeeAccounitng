using AuthService.Core.Entities;
using General.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SavingChangesEventArgs = AuthService.Core.Events.SavingChangesEventArgs;

namespace AuthService.Infrastructure.DbContext;

    public sealed class ApplicationContext : IdentityDbContext<User>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApplicationContext> _logger;
        
        public event EventHandler<SavingChangesEventArgs> SavingChangesEvent;
        
        public ApplicationContext(
            DbContextOptions<ApplicationContext> options,
            ICurrentUserService currentUserService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApplicationContext> logger)
            : base(options)
        {
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        
        public DbSet<ConfirmationCode> ConfirmationCode { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<User>()
                .HasQueryFilter(u => _currentUserService.IsInRole(General.Auth.Roles.RoleAdmin) || (u.IsDeleted == false && u.IsBlocked == false &&
                                                                               _currentUserService.UserId != null && u.Id == _currentUserService.UserId));
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        public override int SaveChanges()
        {
            ApplyAudit();
            DispatchSavingChangesEvent();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            ApplyAudit();
            await DispatchSavingChangesEventAsync();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        
        private void ApplyAudit()
        {
            var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<IAuditable>())
            {
                var entity = entry.Entity;
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedBy = currentUser;
                    entity.CreatedAt = now;
                    entity.UpdatedBy = currentUser;
                    entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedBy = currentUser;
                    entity.UpdatedAt = now;
                }
            }
        }
        
        private void DispatchSavingChangesEvent()
        {
            var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
            var args = new SavingChangesEventArgs(ChangeTracker.Entries(), currentUser);
            if (SavingChangesEvent == null)
            {
                _logger.LogWarning("Нет подписчиков на SavingChangesEvent!");
            }
            else
            {
                _logger.LogInformation($"Вызывается SavingChangesEvent с {SavingChangesEvent.GetInvocationList().Length} подписчиками");
            }
            SavingChangesEvent?.Invoke(this, args);
        }
        
        private Task DispatchSavingChangesEventAsync()
        {
            DispatchSavingChangesEvent();
            return Task.CompletedTask;
        }
    }