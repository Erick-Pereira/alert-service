using Simcag.Shared.Common;

namespace Simcag.AlertService.Domain.Entities;

/// <summary>
/// Entidade que representa um alerta gerado pelo sistema
/// </summary>
public class Alert : BaseEntity
{
    /// <summary>
    /// Identificador único do produto associado ao alerta
    /// </summary>
    public string ProductId { get; private set; } = string.Empty;

    /// <summary>
    /// Nome do produto associado ao alerta
    /// </summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>
    /// Categoria do produto
    /// </summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// Tipo do alerta (DROP, RISE, TREND, CONCENTRATION, etc.)
    /// </summary>
    public string Type { get; private set; } = string.Empty;

    /// <summary>
    /// Categoria do alerta (Superfaturamento, Concentração, etc.)
    /// </summary>
    public string AlertCategory { get; private set; } = string.Empty;

    /// <summary>
    /// Nível de severidade do alerta
    /// </summary>
    public AlertSeverity Severity { get; private set; }

    /// <summary>
    /// Desvio percentual que gerou o alerta
    /// </summary>
    public decimal DeviationPercentage { get; private set; }

    /// <summary>
    /// Mensagem descritiva do alerta
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// Preço atual no momento do alerta
    /// </summary>
    public decimal CurrentPrice { get; private set; }

    /// <summary>
    /// Preço médio histórico
    /// </summary>
    public decimal AveragePrice { get; private set; }

    /// <summary>
    /// Data de criação do alerta
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Data de análise que gerou o alerta
    /// </summary>
    public DateTime AnalyzedAt { get; private set; }

    /// <summary>
    /// Indica se o alerta já foi resolvido
    /// </summary>
    public bool Resolved { get; private set; }

    /// <summary>
    /// Data de resolução do alerta (se aplicável)
    /// </summary>
    public DateTime? ResolvedAt { get; private set; }

    private Alert() { } // EF Core

    /// <summary>
    /// Construtor privado para criação de alertas
    /// </summary>
    private Alert(
        string productId,
        string productName,
        string category,
        string type,
        string alertCategory,
        AlertSeverity severity,
        decimal deviationPercentage,
        string message,
        decimal currentPrice,
        decimal averagePrice,
        DateTime analyzedAt)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        Category = category;
        Type = type;
        AlertCategory = alertCategory;
        Severity = severity;
        DeviationPercentage = deviationPercentage;
        Message = message;
        CurrentPrice = currentPrice;
        AveragePrice = averagePrice;
        AnalyzedAt = analyzedAt;
        CreatedAt = DateTime.UtcNow;
        Resolved = false;
        ResolvedAt = null;
    }

    /// <summary>
    /// Cria um novo alerta
    /// </summary>
    public static Alert Create(
        string productId,
        string productName,
        string category,
        string type,
        string alertCategory,
        AlertSeverity severity,
        decimal deviationPercentage,
        string message,
        decimal currentPrice,
        decimal averagePrice,
        DateTime analyzedAt) =>
        new(productId, productName, category, type, alertCategory, severity,
            deviationPercentage, message, currentPrice, averagePrice, analyzedAt);

    /// <summary>
    /// Marca o alerta como resolvido
    /// </summary>
    public void MarkAsResolved()
    {
        Resolved = true;
        ResolvedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calcula a gravidade do alerta baseado no desvio e na severidade
    /// </summary>
    public int CalculateRiskScore()
    {
        var baseScore = Severity switch
        {
            AlertSeverity.Low => 1,
            AlertSeverity.Medium => 2,
            AlertSeverity.High => 3,
            AlertSeverity.Critical => 4,
            _ => 1
        };

        var deviationMultiplier = Math.Abs((double)DeviationPercentage) / 100.0;
        return (int)Math.Round(baseScore * (1 + deviationMultiplier));
    }
}
