using System;
using System.Collections.Generic;
using System.Text;
using Simcag.AlertService.Domain.Entities;

namespace Simcag.AlertService.Application.Interfaces;

public interface IAlertRepository
{
    Task AddAsync(Alert alert);

    Task<Alert?> GetByIdAsync(Guid id);

    Task<IEnumerable<Alert>> GetAllAsync(int page, int pageSize);

    Task UpdateAsync(Alert alert);
}
