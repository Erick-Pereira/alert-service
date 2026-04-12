using System;
using System.Collections.Generic;
using System.Text;
using Simcag.AlertService.Domain.Entities;

namespace Simcag.AlertService.Application.Interfaces
{
    public interface IAlertRepository
    {
        Task AddAsync(Alert alert);

        Task<Alert?> GetByIdAsync(Guid id);

        Task<IEnumerable<Alert>> GetAllAsync(int page, int pageSize);

        Task<IEnumerable<Alert>> GetFilteredAsync(
            int page,
            int pageSize,
            string? level,
            bool? isRead
        );

        Task<int> CountAsync(bool? isRead = null, string? level = null);

        Task UpdateAsync(Alert alert);
    }
}