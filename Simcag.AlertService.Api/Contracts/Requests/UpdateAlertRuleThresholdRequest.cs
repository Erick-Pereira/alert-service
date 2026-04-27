namespace Simcag.AlertService.Api.Contracts.Requests;

/// <summary>
/// Request DTO para atualização de threshold de regra de alerta
/// </summary>
public class UpdateAlertRuleThresholdRequest
{
    /// <summary>
    /// Novo valor do threshold (deve ser maior que zero)
    /// </summary>
    public decimal NewThreshold { get; set; }
}
