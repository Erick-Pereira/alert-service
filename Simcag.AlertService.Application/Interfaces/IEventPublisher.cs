using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.Interfaces;

/// <summary>
/// Interface para publicação de eventos no sistema de mensageria
/// </summary>
/// <typeparam name="T">Tipo da mensagem</typeparam>
public interface IEventPublisher<T> where T : class
{
    /// <summary>
    /// Publica uma mensagem no sistema de mensageria
    /// </summary>
    /// <param name="message">Mensagem a ser publicada</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Tarefa que representa a conclusão da publicação</returns>
    Task PublishAsync(T message, CancellationToken ct);
}