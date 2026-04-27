using System.Collections.Generic;

namespace Simcag.AlertService.Application.DTOs;

/// <summary>
/// Resultado paginado genérico
/// </summary>
public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
