using MedicalPrescription.Application.DTOs.Prescricoes;

namespace MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;

public interface IListarPrescricoesUseCase
{
    Task<ResultadoPaginado<PrescricaoResumoDto>> ExecutarAsync(
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default);
}
