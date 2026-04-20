namespace Simcag.AlertService.Domain.Enums
{
    /// <summary>
    /// Níveis de severidade para alertas
    /// </summary>
    public enum AlertLevel
    {
        /// <summary>
        /// Alerta normal - desvio dentro do limite aceitável (< 10%)
        /// </summary>
        Normal = 0,
        
        /// <summary>
        /// Alerta suspeito - desvio entre 10% e 30%
        /// </summary>
        Suspeito = 1,
        
        /// <summary>
        /// Alerta superfaturado - desvio acima de 30%
        /// </summary>
        Superfaturado = 2,
        
        /// <summary>
        /// Produto sem preço de mercado para comparação
        /// </summary>
        OutOfStock = 3
    }
}