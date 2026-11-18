using MedicalPrescription.Domain.Exceptions;

using MedicalPrescription.Domain.ValueObjects;

namespace MedicalPrescription.Domain.Entities;

public class PrescricaoItem
{
    public Guid Id { get; private set; }
    public Guid PrescricaoId { get; private set; }
    public Guid? MedicamentoId { get; private set; }

    public string NomeMedicamento { get; private set; } = string.Empty;
    public string FormaFarmaceutica { get; private set; } = string.Empty;
    public Dosagem Dosagem { get; private set; } = null!;
    public string Frequencia { get; private set; } = string.Empty;
    public DateOnly InicioTratamento { get; private set; }
    public int? DuracaoTratamentoDias { get; private set; }
    public bool UsoContinuo { get; private set; }
    public string ViaAdministracao { get; private set; } = string.Empty;
    public string PrazoUtilizacao { get; private set; } = string.Empty;
    public string? Observacao { get; private set; }

    private PrescricaoItem() { }

    public PrescricaoItem(
        Guid? medicamentoId,
        string nomeMedicamento,
        string formaFarmaceutica,
        decimal dose,
        string unidadeMedida,
        string frequencia,
        DateOnly inicioTratamento,
        int? duracaoTratamentoDias,
        bool usoContinuo,
        string viaAdministracao,
        string prazoUtilizacao,
        string? observacao = null)
    {
        if (medicamentoId == Guid.Empty)
            throw new ArgumentException("Medicamento inválido.");

        if (inicioTratamento == default)
            throw new ArgumentException("Data de início inválida.");

        if (duracaoTratamentoDias.HasValue &&
            duracaoTratamentoDias.Value <= 0)
            throw new ArgumentException("Duração inválida.");

        if (!usoContinuo && !duracaoTratamentoDias.HasValue)
            throw new ArgumentException(
                "Informe a duração para tratamentos não contínuos.");

        Id = Guid.NewGuid();
        MedicamentoId = medicamentoId;

        NomeMedicamento = Obrigatorio(
            nomeMedicamento, nameof(nomeMedicamento));

        FormaFarmaceutica = Obrigatorio(
            formaFarmaceutica, nameof(formaFarmaceutica));

        Dosagem = new Dosagem(dose, unidadeMedida);

        Frequencia = Obrigatorio(
            frequencia, nameof(frequencia));

        InicioTratamento = inicioTratamento;
        DuracaoTratamentoDias = duracaoTratamentoDias;
        UsoContinuo = usoContinuo;

        ViaAdministracao = Obrigatorio(
            viaAdministracao, nameof(viaAdministracao));

        PrazoUtilizacao = Obrigatorio(
            prazoUtilizacao, nameof(prazoUtilizacao));

        Observacao = observacao?.Trim();
    }

    internal void VincularPrescricao(Guid prescricaoId)
    {
        if (prescricaoId == Guid.Empty)
            throw new ArgumentException("Prescrição inválida.");

        if (PrescricaoId != Guid.Empty &&
            PrescricaoId != prescricaoId)
            throw new RegraNegocioException(
                "O item já pertence a outra prescrição.");

        PrescricaoId = prescricaoId;
    }

    private static string Obrigatorio(string? valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException(
                $"O campo {campo} é obrigatório.");

        return valor.Trim();
    }
}
