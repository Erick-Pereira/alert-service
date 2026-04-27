using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.Interfaces;

/// <summary>
/// Interface para operações de cache de alertas
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Verifica se um alerta é duplicado (cooldown)
    /// </summary>
    /// <param name="entityType">Tipo de entidade</param>
    /// <param name="entityId">ID da entidade</param>
    /// <param name="cooldown">Tempo de cooldown</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Verdadeiro se é duplicado</returns>
    Task<bool> IsDuplicateAlertAsync(
        string entityType,
        string entityId,
        TimeSpan cooldown,
        CancellationToken ct);

    /// <summary>
    /// Marca um alerta como enviado
    /// </summary>
    /// <param name="entityType">Tipo de entidade</param>
    /// <param name="entityId">ID da entidade</param>
    /// <param name="ttl">Tempo de vida útil</param>
    /// <param name="ct">Token de cancelamento</param>
    Task MarkAlertAsSentAsync(
        string entityType,
        string entityId,
        TimeSpan ttl,
        CancellationToken ct);
}