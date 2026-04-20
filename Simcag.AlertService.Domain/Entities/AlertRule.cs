using System;
using System.Collections.Generic;
using System.Text;

namespace Simcag.AlertService.Domain.Entities
{
    /// <summary>
    /// Regra de alerta configurável para detecção de anomalias.
    /// </summary>
    public class AlertRule
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public AlertSeverity Severity { get; private set; }
        public decimal DeviationThreshold { get; private set; } // Desvio percentual mínimo para alerta
        public TimeSpan? TimeWindow { get; private set; } // Janela de tempo para análise
        public List<string> ProductCategories { get; private set; } = new();
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastEvaluatedAt { get; private set; }
        public int EvaluationCount { get; private set; }
        public int AlertTriggeredCount { get; private set; }

        private AlertRule() { }

        public static AlertRule Create(string name, string description, AlertSeverity severity, decimal deviationThreshold, TimeSpan? timeWindow = null, List<string>? productCategories = null)
        {
            return new AlertRule
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Severity = severity,
                DeviationThreshold = deviationThreshold,
                TimeWindow = timeWindow,
                ProductCategories = productCategories ?? new List<string>(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Evaluate(decimal deviationPercentage)
        {
            if (deviationPercentage >= DeviationThreshold)
            {
                LastEvaluatedAt = DateTime.UtcNow;
                EvaluationCount++;
                AlertTriggeredCount++;
            }
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
