using Simcag.AlertService.Domain.Entities;
using Simcag.Shared.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.Interfaces;

/// <summary>
/// Serviço principal para gerenciamento de alertas
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Avalia um evento de análise de preço e gera alertas
    /// </summary>
    /// <param name="analysisEvent">O evento de análise de preço</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>O alerta gerado ou null se nenhuma regra foi disparada</returns>
    Task<Alert?> EvaluateAlertAsync(
        PriceAnalyzedEvent analysisEvent,
        CancellationToken ct);

    /// <summary>
    /// Obtém todas as regras de alerta ativas
    /// </summary>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Lista de regras ativas</returns>
    Task<IEnumerable<AlertRule>> GetActiveRulesAsync(CancellationToken ct);

    /// <summary>
    /// Obtém um alerta por ID
    /// </summary>
    /// <param name="id">ID do alerta</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>O alerta ou null se não encontrado</returns>
    Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Obtém alertas de um produto específico
    /// </summary>
    /// <param name="productId">ID do produto</param>
    /// <param name="from">Data de início</param>
    /// <param name="to">Data de fim</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Lista de alertos do produto</returns>
    Task<IEnumerable<Alert>> GetAlertsByProductAsync(
        string productId,
        DateTime from,
        DateTime to,
        CancellationToken ct);

    /// <summary>
    /// Atualiza o threshold de uma regra de alerta
    /// </summary>
    /// <param name="ruleId">ID da regra</param>
    /// <param name="newThreshold">Novo threshold</param>
    /// <param name="ct">Token de cancelamento</param>
    Task UpdateAlertRuleThresholdAsync(
        Guid ruleId,
        decimal newThreshold,
        CancellationToken ct);
}