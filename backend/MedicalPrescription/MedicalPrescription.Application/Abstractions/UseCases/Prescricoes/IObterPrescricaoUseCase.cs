using MedicalPrescription.Application.DTOs.Prescricoes;

namespace MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;

public interface IObterPrescricaoUseCase
{
    Task<PrescricaoDetalheDto> ExecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
