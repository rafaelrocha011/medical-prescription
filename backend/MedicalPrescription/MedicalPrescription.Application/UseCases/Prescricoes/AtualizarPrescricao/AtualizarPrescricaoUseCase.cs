using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;
using MedicalPrescription.Application.DTOs.Prescricoes;
using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Domain.Enums;

namespace MedicalPrescription.Application.UseCases.Prescricoes.AtualizarPrescricao;

public sealed class AtualizarPrescricaoUseCase
    : IAtualizarPrescricaoUseCase
{
    private readonly IPrescricaoRepository _prescricoes;
    private readonly IMedicamentoRepository _medicamentos;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarPrescricaoUseCase(
        IPrescricaoRepository prescricoes,
        IMedicamentoRepository medicamentos,
        IUnitOfWork unitOfWork)
    {
        _prescricoes = prescricoes;
        _medicamentos = medicamentos;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecutarAsync(
        Guid id,
        AtualizarPrescricaoRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (id == Guid.Empty)
            throw new ArgumentException("Prescricao invalida.");

        var prescricao = await _prescricoes.ObterPorIdAsync(
            id, cancellationToken);

        if (prescricao is null)
            throw new KeyNotFoundException(
                "Prescricao nao encontrada.");

        switch (prescricao.Tipo)
        {
            case TipoPrescricao.NaoEstruturada:

                if (request.Itens?.Count > 0)
                    throw new ArgumentException(
                        "Receita nao estruturada nao aceita itens.");

                prescricao.AtualizarTextoLivre(
                    request.TextoLivre ?? string.Empty);

                break;

            case TipoPrescricao.Estruturada:

                if (!string.IsNullOrWhiteSpace(request.TextoLivre))
                    throw new ArgumentException(
                        "Receita estruturada nao aceita texto livre.");

                if (request.Itens is null || request.Itens.Count == 0)
                    throw new ArgumentException(
                        "Informe pelo menos um medicamento.");

                var itens = new List<PrescricaoItem>();

                foreach (var entrada in request.Itens)
                {
                    ArgumentNullException.ThrowIfNull(entrada);

                    var nome = entrada.NomeMedicamento;

                    if (entrada.MedicamentoId is Guid medicamentoId)
                    {
                        if (medicamentoId == Guid.Empty)
                            throw new ArgumentException(
                                "Medicamento invalido.");

                        var medicamento = await _medicamentos
                            .ObterPorIdAsync(
                                medicamentoId,
                                cancellationToken);

                        if (medicamento is null)
                            throw new KeyNotFoundException(
                                "Medicamento nao encontrado.");

                        nome = medicamento.Nome;
                    }

                    itens.Add(new PrescricaoItem(
                        medicamentoId: entrada.MedicamentoId,
                        nomeMedicamento: nome,
                        formaFarmaceutica: entrada.FormaFarmaceutica,
                        dose: entrada.Dose,
                        unidadeMedida: entrada.UnidadeMedida,
                        frequencia: entrada.Frequencia,
                        inicioTratamento: entrada.InicioTratamento,
                        duracaoTratamentoDias: entrada.DuracaoTratamentoDias,
                        usoContinuo: entrada.UsoContinuo,
                        viaAdministracao: entrada.ViaAdministracao,
                        prazoUtilizacao: entrada.PrazoUtilizacao,
                        observacao: entrada.Observacao
                    ));
                }

                prescricao.SubstituirItens(itens);
                break;

            default:
                throw new InvalidOperationException(
                    "Tipo de prescricao nao suportado.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
