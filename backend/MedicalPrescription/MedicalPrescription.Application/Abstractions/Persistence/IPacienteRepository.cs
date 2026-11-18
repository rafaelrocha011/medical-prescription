using MedicalPrescription.Application.DTOs.Catalogos;

namespace MedicalPrescription.Application.Abstractions.Persistence;

public interface IPacienteRepository
{
    Task<bool> ExisteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PacienteResumoDto>> ListarAsync(
        string? busca,
        CancellationToken cancellationToken = default);
}