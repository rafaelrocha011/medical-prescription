using MedicalPrescription.Application.DTOs.Catalogos;

namespace MedicalPrescription.Application.Abstractions.UseCases.Catalogos;

public interface IListarMedicamentosUseCase
{
    Task<IReadOnlyList<MedicamentoResumoDto>> ExecutarAsync(
        string? busca,
        CancellationToken cancellationToken = default);
}
