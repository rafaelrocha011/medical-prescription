
using MedicalPrescription.Domain.Entities;

namespace MedicalPrescription.Application.Abstractions.Persistence;

public interface IPrescricaoWriteRepository
{
    Task<Prescricao?> ObterParaEscritaAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(
        Prescricao prescricao,
        CancellationToken cancellationToken = default);

    void Remover(Prescricao prescricao);
}
