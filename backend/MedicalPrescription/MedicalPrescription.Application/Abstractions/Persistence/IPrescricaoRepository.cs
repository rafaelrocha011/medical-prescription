
using MedicalPrescription.Domain.Entities;

namespace MedicalPrescription.Application.Abstractions.Persistence;

public interface IPrescricaoRepository
{
    Task<Prescricao?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(
        Prescricao prescricao,
        CancellationToken cancellationToken = default);

    void Remover(Prescricao prescricao);
}
