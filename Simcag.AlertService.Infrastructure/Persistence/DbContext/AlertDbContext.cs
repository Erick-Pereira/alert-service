using Microsoft.EntityFrameworkCore;
using Simcag.AlertService.Domain.Entities;

namespace Simcag.AlertService.Infrastructure.Persistence.DbContext;

public class AlertDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public AlertDbContext(DbContextOptions<AlertDbContext> options)
        : base(options)
    {
    }

    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.ProductId).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Type).IsRequired().HasMaxLength(20);
            entity.Property(a => a.Message).IsRequired().HasMaxLength(500);
            entity.Property(a => a.CreatedAt).IsRequired();
            entity.HasIndex(a => a.ProductId);
            entity.HasIndex(a => a.CreatedAt);
            entity.HasIndex(a => a.Resolved);
        });

        modelBuilder.Entity<AlertRule>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
            entity.Property(r => r.Description).HasMaxLength(1000);
            entity.Property(r => r.Category).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Type).IsRequired();
            entity.Property(r => r.Threshold).HasPrecision(18, 2);
            entity.HasIndex(r => r.Category);
            entity.HasIndex(r => r.Type);
            entity.HasIndex(r => r.IsEnabled);
        });
    }
}
