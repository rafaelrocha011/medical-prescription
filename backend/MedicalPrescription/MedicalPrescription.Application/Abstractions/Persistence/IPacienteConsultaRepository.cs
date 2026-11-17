using MedicalPrescription.Application.DTOs.Catalogos;

namespace MedicalPrescription.Application.Abstractions.Persistence;

public interface IPacienteConsultaRepository
{
    Task<IReadOnlyList<PacienteResumoDto>> ListarAsync(
        string? busca,
        CancellationToken cancellationToken = default);
}
