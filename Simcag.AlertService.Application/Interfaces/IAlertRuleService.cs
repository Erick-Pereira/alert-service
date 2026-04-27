using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.ValueObjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.Interfaces;

/// <summary>
/// Interface para regras de alerta do domínio (Application layer)
/// </summary>
public interface IAlertRuleService
{
    /// <summary>
    /// Obtém regras aplicáveis para uma categoria e fornecedor
    /// </summary>
    /// <param name="category">Categoria do produto</param>
    /// <param name="supplierId">ID do fornecedor (opcional)</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Lista de regras aplicáveis</returns>
    Task<IEnumerable<AlertRule>> GetApplicableRulesAsync(
        string category,
        string? supplierId,
        CancellationToken ct);

    /// <summary>
    /// Calcula o desvio percentual de um valor
    /// </summary>
    /// <param name="currentValue">Valor atual</param>
    /// <param name="baseline">Valor de base</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Desvio percentual calculado</returns>
    Task<DeviationPercentage> CalculateDeviationAsync(
        decimal currentValue,
        decimal baseline,
        CancellationToken ct);

    /// <summary>
    /// Classifica a severidade de um desvio percentual
    /// </summary>
    /// <param name="deviationPercentage">Desvio percentual</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Classificação de severidade</returns>
    Task<AlertClassification> ClassifyDeviationAsync(
        decimal deviationPercentage,
        CancellationToken ct);
}