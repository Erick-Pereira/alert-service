using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Simcag.AlertService.Application.DTOs;
using Simcag.AlertService.Domain.Entities;

namespace Simcag.AlertService.Application.Interfaces;

/// <summary>
/// Repositório para persistência de alertas
/// </summary>
public interface IAlertRepository
{
    Task AddAsync(Alert alert, CancellationToken ct);
    /// <summary>Leitura por id (rastreio inativo, para projeção GET).</summary>
    Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Carregamento rastreado (atualizar estado, ex. marcar lido/encerrado).</summary>
    Task<Alert?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);

    Task<Alert?> GetLastByProductIdAsync(string productId, CancellationToken ct);

    /// <summary>Paginação no banco, filtro opcional de tipo (case-insensitive no PostgreSQL via tradução).</summary>
    Task<AlertListQueryResult> GetPageAsync(
        int page, int pageSize, string? type, CancellationToken ct);

    /// <summary>Agregações (total, por tipo, não resolvidos) sem varrer a tabela toda na API.</summary>
    Task<AlertStatsQueryResult> GetStatsAsync(CancellationToken ct);

    Task UpdateAsync(Alert alert, CancellationToken ct);

    Task<IEnumerable<Alert>> GetByProductAsync(
        string productId,
        DateTime from,
        DateTime to,
        CancellationToken ct);
}

