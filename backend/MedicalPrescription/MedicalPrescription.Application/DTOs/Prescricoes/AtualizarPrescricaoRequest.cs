namespace MedicalPrescription.Application.DTOs.Prescricoes;

public sealed record AtualizarPrescricaoRequest(
    string? TextoLivre,
    IReadOnlyCollection<CriarPrescricaoItemRequest>? Itens
);
