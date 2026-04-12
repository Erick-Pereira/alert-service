<<<<<<< HEAD
﻿using Microsoft.AspNetCore.Mvc;
using Simcag.AlertService.Application.UseCases.GetAlerts;
using Simcag.AlertService.Application.UseCases.GetAlertById;
using Simcag.AlertService.Application.UseCases.MarkAlertAsRead;
using Simcag.AlertService.Application.UseCases.GetAlertStats;

namespace alert_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly GetAlertsHandler _getAlerts;
        private readonly GetAlertByIdHandler _getById;
        private readonly MarkAlertAsReadHandler _markAsRead;
        private readonly GetAlertStatsHandler _stats;

        public AlertsController(
            GetAlertsHandler getAlerts,
            GetAlertByIdHandler getById,
            MarkAlertAsReadHandler markAsRead,
            GetAlertStatsHandler stats)
        {
            _getAlerts = getAlerts;
            _getById = getById;
            _markAsRead = markAsRead;
            _stats = stats;
        }

        [HttpGet]
        public async Task<IActionResult> GetAlerts(
            int page = 1,
            int pageSize = 10,
            string? level = null,
            bool? isRead = null)
        {
            var result = await _getAlerts.Handle(new GetAlertsQuery
            {
                Page = page,
                PageSize = pageSize,
                Level = level,
                IsRead = isRead
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _getById.Handle(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var success = await _markAsRead.Handle(id);

            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var result = await _stats.Handle();
            return Ok(result);
        }
    }
}
=======
﻿namespace alert_service.Controllers
{
    public class AlertsController
    {
    }
}
>>>>>>> 23a5c09dab3fb6f834f5f4642538e5640262907f
