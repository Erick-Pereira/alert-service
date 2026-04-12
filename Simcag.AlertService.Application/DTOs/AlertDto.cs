using System;
using System.Collections.Generic;
using System.Text;
using Simcag.AlertService.Application.DTOs;

namespace Simcag.AlertService.Application.DTOs
{
    public class AlertDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public string Level { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}