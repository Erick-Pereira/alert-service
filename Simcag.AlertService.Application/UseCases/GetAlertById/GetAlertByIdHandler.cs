using Simcag.AlertService.Application.DTOs;
using Simcag.AlertService.Application.Interfaces;

namespace Simcag.AlertService.Application.UseCases.GetAlertById
{
    public class GetAlertByIdHandler
    {
        private readonly IAlertRepository _repository;

        public GetAlertByIdHandler(IAlertRepository repository)
        {
            _repository = repository;
        }

        public async Task<AlertDto?> Handle(Guid id)
        {
            var alert = await _repository.GetByIdAsync(id);

            if (alert == null) return null;

            return new AlertDto
            {
                Id = alert.Id,
                Message = $"{alert.ProductName} price deviation",
                Level = alert.AlertLevel,
                IsRead = alert.IsRead,
                CreatedAt = alert.CreatedAt
            };
        }
    }
}