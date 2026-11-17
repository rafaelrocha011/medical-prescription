using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;
using MedicalPrescription.Application.DTOs.Prescricoes;

namespace MedicalPrescription.Application.UseCases.Prescricoes.ObterPrescricao;

public sealed class ObterPrescricaoUseCase : IObterPrescricaoUseCase
{
    private readonly IPrescricaoRepository _repository;

    public ObterPrescricaoUseCase(IPrescricaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<PrescricaoDetalheDto> ExecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Prescricao invalida.");

        var prescricao = await _repository.ObterPorIdAsync(
            id, cancellationToken);

        if (prescricao is null)
            throw new KeyNotFoundException("Prescricao nao encontrada.");

        var itens = prescricao.Itens.Select(item =>
            new PrescricaoItemDto(
                item.Id,
                item.MedicamentoId,
                item.NomeMedicamento,
                item.FormaFarmaceutica,
                item.Dosagem.Quantidade,
                item.Dosagem.UnidadeMedida,
                item.Frequencia,
                item.InicioTratamento,
                item.DuracaoTratamentoDias,
                item.UsoContinuo,
                item.ViaAdministracao,
                item.PrazoUtilizacao,
                item.Observacao
            )).ToArray();

        return new PrescricaoDetalheDto(
            prescricao.Id,
            prescricao.PacienteId,
            prescricao.Tipo,
            prescricao.TextoLivre,
            prescricao.CriadaEmUtc,
            itens
        );
    }
}
