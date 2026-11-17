using MedicalPrescription.Application.DTOs.Catalogos;

namespace MedicalPrescription.Application.Abstractions.Persistence;

public interface IMedicamentoConsultaRepository
{
    Task<IReadOnlyList<MedicamentoResumoDto>> ListarAsync(
        string? busca,
        CancellationToken cancellationToken = default);
}
