using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.DTOs.Prescricoes;
using MedicalPrescription.Domain.Entities;

namespace MedicalPrescription.Application.Commands.Prescricoes.Shared;

internal static class PrescricaoItemFactory
{
    public static async Task<List<PrescricaoItem>> CriarAsync(
        IReadOnlyCollection<CriarPrescricaoItemRequest> entradas,
        IMedicamentoRepository medicamentos,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entradas);
        ArgumentNullException.ThrowIfNull(medicamentos);

        var itens = new List<PrescricaoItem>(entradas.Count);

        foreach (var entrada in entradas)
        {
            ArgumentNullException.ThrowIfNull(entrada);

            var nome = entrada.NomeMedicamento;

            if (entrada.MedicamentoId is Guid medicamentoId)
            {
                if (medicamentoId == Guid.Empty)
                    throw new ArgumentException(
                        "Identificador de medicamento invalido.");

                var medicamento = await medicamentos.ObterPorIdAsync(
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

        return itens;
    }
}