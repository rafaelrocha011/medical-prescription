using MedicalPrescription.Application.Commands.Prescricoes.Shared;
using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;
using MedicalPrescription.Application.DTOs.Prescricoes;
using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Domain.Enums;

namespace MedicalPrescription.Application.Commands.Prescricoes.CriarPrescricao;

public sealed class CriarPrescricaoUseCase : ICriarPrescricaoUseCase
{
    private readonly IPacienteRepository _pacientes;
    private readonly IMedicamentoRepository _medicamentos;
    private readonly IPrescricaoWriteRepository _prescricoes;
    private readonly IUnitOfWork _unitOfWork;

    public CriarPrescricaoUseCase(
        IPacienteRepository pacientes,
        IMedicamentoRepository medicamentos,
        IPrescricaoWriteRepository prescricoes,
        IUnitOfWork unitOfWork)
    {
        _pacientes = pacientes;
        _medicamentos = medicamentos;
        _prescricoes = prescricoes;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ExecutarAsync(
        CriarPrescricaoRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.PacienteId == Guid.Empty)
            throw new ArgumentException("Paciente invalido.");

        var pacienteExiste = await _pacientes.ExisteAsync(
            request.PacienteId,
            cancellationToken);

        if (!pacienteExiste)
            throw new KeyNotFoundException(
                "Paciente nao encontrado.");

        Prescricao prescricao;

        switch (request.Tipo)
        {
            case TipoPrescricao.NaoEstruturada:

                if (request.Itens?.Count > 0)
                    throw new ArgumentException(
                        "Receita nao estruturada nao aceita itens.");

                prescricao = Prescricao.CriarNaoEstruturada(
                    request.PacienteId,
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
prescricao = Prescricao.CriarEstruturada(
                    request.PacienteId,
                    itens);

                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(request.Tipo),
                    "Tipo de prescricao invalido.");
        }

        await _prescricoes.AdicionarAsync(
            prescricao,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return prescricao.Id;
    }
}
