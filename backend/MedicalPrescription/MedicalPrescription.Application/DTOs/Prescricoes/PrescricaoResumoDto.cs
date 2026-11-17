using MedicalPrescription.Domain.Enums;

namespace MedicalPrescription.Application.DTOs.Prescricoes;

public sealed record PrescricaoResumoDto(
    Guid Id,
    Guid PacienteId,
    TipoPrescricao Tipo,
    DateTime CriadaEmUtc,
    int QuantidadeMedicamentos
);
