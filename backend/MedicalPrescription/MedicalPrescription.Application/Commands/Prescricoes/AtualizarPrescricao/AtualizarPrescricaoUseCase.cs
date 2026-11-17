using MedicalPrescription.Application.Commands.Prescricoes.Shared;
using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;
using MedicalPrescription.Application.DTOs.Prescricoes;
using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Domain.Enums;

namespace MedicalPrescription.Application.Commands.Prescricoes.AtualizarPrescricao;

public sealed class AtualizarPrescricaoUseCase
    : IAtualizarPrescricaoUseCase
{
    private readonly IPrescricaoWriteRepository _prescricoes;
    private readonly IMedicamentoRepository _medicamentos;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarPrescricaoUseCase(
        IPrescricaoWriteRepository prescricoes,
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

        var prescricao = await _prescricoes.ObterParaEscritaAsync(
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

                var itens = await PrescricaoItemFactory.CriarAsync(
                    request.Itens,
                    _medicamentos,
                    cancellationToken);
prescricao.SubstituirItens(itens);
                break;

            default:
                throw new InvalidOperationException(
                    "Tipo de prescricao nao suportado.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
