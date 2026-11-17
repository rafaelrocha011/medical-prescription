using MedicalPrescription.Application.DTOs.Prescricoes;

namespace MedicalPrescription.Application.Abstractions.Persistence;

public interface IPrescricaoConsultaRepository
{
    Task<ResultadoPaginado<PrescricaoResumoDto>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default);
}
