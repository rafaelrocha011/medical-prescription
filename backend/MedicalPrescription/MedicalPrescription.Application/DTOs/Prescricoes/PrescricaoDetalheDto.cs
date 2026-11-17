using MedicalPrescription.Domain.Enums;

namespace MedicalPrescription.Application.DTOs.Prescricoes;

public sealed record PrescricaoDetalheDto(
    Guid Id,
    Guid PacienteId,
    TipoPrescricao Tipo,
    string? TextoLivre,
    DateTime CriadaEmUtc,
    IReadOnlyList<PrescricaoItemDto> Itens
);
