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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.ProductId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.Type)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(a => a.Message)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(a => a.CreatedAt)
                .IsRequired();

            entity.HasIndex(a => a.ProductId);
            entity.HasIndex(a => a.CreatedAt);
        });
    }
}
