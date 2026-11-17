using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;
using MedicalPrescription.Application.DTOs.Prescricoes;

namespace MedicalPrescription.Application.UseCases.Prescricoes.ListarPrescricoes;

public sealed class ListarPrescricoesUseCase
    : IListarPrescricoesUseCase
{
    private const int TamanhoMaximoPagina = 100;

    private readonly IPrescricaoConsultaRepository _repository;

    public ListarPrescricoesUseCase(
        IPrescricaoConsultaRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultadoPaginado<PrescricaoResumoDto>> ExecutarAsync(
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        if (pagina < 1)
            throw new ArgumentOutOfRangeException(
                nameof(pagina),
                "A pagina deve ser maior ou igual a 1.");

        if (tamanhoPagina < 1 || tamanhoPagina > TamanhoMaximoPagina)
            throw new ArgumentOutOfRangeException(
                nameof(tamanhoPagina),
                "O tamanho da pagina deve estar entre 1 e 100.");

        return await _repository.ListarAsync(
            pagina,
            tamanhoPagina,
            cancellationToken);
    }
}
