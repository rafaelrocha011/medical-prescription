using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;
using MedicalPrescription.Application.DTOs.Prescricoes;
using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Domain.Enums;

namespace MedicalPrescription.Application.UseCases.Prescricoes.CriarPrescricao;

public sealed class CriarPrescricaoUseCase : ICriarPrescricaoUseCase
{
    private readonly IPacienteRepository _pacientes;
    private readonly IMedicamentoRepository _medicamentos;
    private readonly IPrescricaoRepository _prescricoes;
    private readonly IUnitOfWork _unitOfWork;

    public CriarPrescricaoUseCase(
        IPacienteRepository pacientes,
        IMedicamentoRepository medicamentos,
        IPrescricaoRepository prescricoes,
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

                var itens = new List<PrescricaoItem>();

                foreach (var entrada in request.Itens)
                {
                    ArgumentNullException.ThrowIfNull(entrada);

                    var nome = entrada.NomeMedicamento;

                    if (entrada.MedicamentoId is Guid medicamentoId)
                    {
                        if (medicamentoId == Guid.Empty)
                            throw new ArgumentException(
                                "Identificador de medicamento invalido.");

                        var medicamento = await _medicamentos
                            .ObterPorIdAsync(
                                medicamentoId,
                                cancellationToken);

                        if (medicamento is null)
                            throw new KeyNotFoundException(
                                "Medicamento nao encontrado.");

                        // O catalogo e a fonte do nome cadastrado.
                        nome = medicamento.Nome;
                    }

                    var item = new PrescricaoItem(
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
                    );

                    itens.Add(item);
                }

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
