using MedicalPrescription.Domain.Entities;

namespace MedicalPrescription.Application.Abstractions.Persistence;

public interface IMedicamentoRepository
{
    Task<Medicamento?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
