using System;
using System.Collections.Generic;
using System.Text;
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

            entity.Property(a => a.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(a => a.OriginalPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(a => a.MarketPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(a => a.DeviationPercentage)
                .HasColumnType("decimal(5,2)");
        });
    }
}
