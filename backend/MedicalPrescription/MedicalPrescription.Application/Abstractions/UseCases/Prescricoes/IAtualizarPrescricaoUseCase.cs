using MedicalPrescription.Application.DTOs.Prescricoes;

namespace MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;

public interface IAtualizarPrescricaoUseCase
{
    Task ExecutarAsync(
        Guid id,
        AtualizarPrescricaoRequest request,
        CancellationToken cancellationToken = default);
}
