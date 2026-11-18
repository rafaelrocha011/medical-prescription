using MedicalPrescription.Application.DTOs.Prescricoes;

namespace MedicalPrescription.Application.Abstractions.Persistence;

public interface IPrescricaoReadRepository
{
    Task<PrescricaoDetalheDto?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<PrescricaoResumoDto>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default);
}
