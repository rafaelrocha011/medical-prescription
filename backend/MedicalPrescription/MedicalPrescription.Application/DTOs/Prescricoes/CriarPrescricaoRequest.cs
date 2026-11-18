
using MedicalPrescription.Domain.Enums;

namespace MedicalPrescription.Application.DTOs.Prescricoes;

public sealed record CriarPrescricaoRequest(
    Guid PacienteId,
    TipoPrescricao Tipo,
    string? TextoLivre,
    IReadOnlyCollection<CriarPrescricaoItemRequest>? Itens
);
