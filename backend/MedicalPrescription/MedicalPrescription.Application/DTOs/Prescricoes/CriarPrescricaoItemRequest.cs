
namespace MedicalPrescription.Application.DTOs.Prescricoes;

public sealed record CriarPrescricaoItemRequest(
    Guid? MedicamentoId,
    string NomeMedicamento,
    string FormaFarmaceutica,
    decimal Dose,
    string UnidadeMedida,
    string Frequencia,
    DateOnly InicioTratamento,
    int? DuracaoTratamentoDias,
    bool UsoContinuo,
    string ViaAdministracao,
    string PrazoUtilizacao,
    string? Observacao
);
