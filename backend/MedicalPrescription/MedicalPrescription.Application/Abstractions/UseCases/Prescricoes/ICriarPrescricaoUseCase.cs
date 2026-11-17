using MedicalPrescription.Application.DTOs.Prescricoes;

namespace MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;

public interface ICriarPrescricaoUseCase
{
    Task<Guid> ExecutarAsync(
        CriarPrescricaoRequest request,
        CancellationToken cancellationToken = default);
}
