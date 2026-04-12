using System;
using System.Collections.Generic;
using System.Text;
using Simcag.AlertService.Application.DTOs;

namespace Simcag.AlertService.Application.DTOs
{
    public class AlertStatsDto
    {
        public int Total { get; set; }
        public int Read { get; set; }
        public int Unread { get; set; }
    }
}