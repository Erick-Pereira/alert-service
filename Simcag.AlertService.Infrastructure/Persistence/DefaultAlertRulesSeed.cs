using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Infrastructure.Persistence.DbContext;

namespace Simcag.AlertService.Infrastructure.Persistence;

/// <summary>
/// Garante regras globais mínimas para o motor de alertas em ambientes locais/dev.
/// </summary>
public static class DefaultAlertRulesSeed
{
    public static async Task EnsureSeededAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AlertDbContext>();
        var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DefaultAlertRulesSeed");

        if (await db.AlertRules.AnyAsync(ct))
            return;

        var rules = new[]
        {
            AlertRule.Create(
                "Superfaturamento vs mercado",
                "Alerta quando o preço da NF excede o benchmark de mercado além do limiar.",
                category: string.Empty,
                supplierId: null,
                AlertType.OverpriceMarket,
                threshold: 15m,
                AlertSeverity.Medium),
            AlertRule.Create(
                "Desvio vs histórico interno",
                "Alerta quando o preço diverge da média histórica interna do produto.",
                category: string.Empty,
                supplierId: null,
                AlertType.OverpriceHistorical,
                threshold: 15m,
                AlertSeverity.Medium),
        };

        db.AlertRules.AddRange(rules);
        await db.SaveChangesAsync(ct);
        log.LogInformation("Seed: {Count} regra(s) padrão de alerta criadas.", rules.Length);
    }
}
