using MedicalPrescription.Application.DTOs.Catalogos;

namespace MedicalPrescription.Application.Abstractions.UseCases.Catalogos;

public interface IListarPacientesUseCase
{
    Task<IReadOnlyList<PacienteResumoDto>> ExecutarAsync(
        string? busca,
        CancellationToken cancellationToken = default);
}
