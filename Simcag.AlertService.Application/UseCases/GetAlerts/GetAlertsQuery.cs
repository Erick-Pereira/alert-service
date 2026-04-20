using System;
using System.Collections.Generic;
using System.Text;

namespace Simcag.AlertService.Application.UseCases.GetAlerts
{
    public class GetAlertsQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Level { get; set; }
        public bool? IsRead { get; set; }
    }
}
