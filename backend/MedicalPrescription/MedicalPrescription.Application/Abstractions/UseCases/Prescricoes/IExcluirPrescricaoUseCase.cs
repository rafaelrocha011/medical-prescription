namespace MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;

public interface IExcluirPrescricaoUseCase
{
    Task ExecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
