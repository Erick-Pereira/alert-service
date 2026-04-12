<<<<<<< HEAD
﻿using Simcag.AlertService.Application.DTOs;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;

namespace Simcag.AlertService.Application.UseCases.GetAlerts;

public class GetAlertsHandler
{
    private readonly IAlertRepository _repository;

    public GetAlertsHandler(IAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<AlertDto>> Handle(GetAlertsQuery query)
    {
        var alerts = await _repository.GetFilteredAsync(
            query.Page,
            query.PageSize,
            query.Level,
            query.IsRead
        );

        var total = await _repository.CountAsync(query.IsRead, query.Level);

        return new PaginatedResult<AlertDto>
        {
            Items = alerts.Select(a => new AlertDto
            {
                Id = a.Id,
                Message = $"{a.ProductName} price deviation",
                Level = a.AlertLevel,
                IsRead = a.IsRead,
                CreatedAt = a.CreatedAt
            }),
            TotalItems = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }
}
=======
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Simcag.AlertService.Application.UseCases.GetAlerts
{
    internal class GetAlertsHandler
    {
    }
}
>>>>>>> 23a5c09dab3fb6f834f5f4642538e5640262907f
