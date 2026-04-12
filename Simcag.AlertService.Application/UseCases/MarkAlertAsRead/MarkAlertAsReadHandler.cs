using Simcag.AlertService.Application.Interfaces;

namespace Simcag.AlertService.Application.UseCases.MarkAlertAsRead
{
	public class MarkAlertAsReadHandler
	{
		private readonly IAlertRepository _repository;

		public MarkAlertAsReadHandler(IAlertRepository repository)
		{
			_repository = repository;
		}

		public async Task<bool> Handle(Guid id)
		{
			var alert = await _repository.GetByIdAsync(id);

			if (alert == null) return false;

			alert.MarkAsRead();
			await _repository.UpdateAsync(alert);

			return true;
		}
	}
}