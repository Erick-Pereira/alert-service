using System;
using System.Collections.Generic;
using Simcag.AlertService.Domain.Entities;

namespace Simcag.AlertService.Application.DTOs;

public sealed record AlertListQueryResult(
    IReadOnlyList<Alert> Items,
    int TotalCount);

public sealed record AlertStatsQueryResult(
    int Total,
    IReadOnlyDictionary<string, int> ByType,
    int UnreadCount);
