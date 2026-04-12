using Simcag.AlertService.Application.DTOs;
using Simcag.AlertService.Application.Interfaces;

namespace Simcag.AlertService.Application.UseCases.GetAlertStats
{
	public class GetAlertStatsHandler
	{
		private readonly IAlertRepository _repository;

		public GetAlertStatsHandler(IAlertRepository repository)
		{
			_repository = repository;
		}

		public async Task<AlertStatsDto> Handle()
		{
			var total = await _repository.CountAsync();
			var read = await _repository.CountAsync(true);
			var unread = await _repository.CountAsync(false);

			return new AlertStatsDto
			{
				Total = total,
				Read = read,
				Unread = unread
			};
		}
	}
}