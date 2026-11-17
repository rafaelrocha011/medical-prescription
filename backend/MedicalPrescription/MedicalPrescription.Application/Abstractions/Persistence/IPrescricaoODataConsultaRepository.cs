using MedicalPrescription.Application.DTOs.Prescricoes;

namespace MedicalPrescription.Application.Abstractions.Persistence;

public interface IPrescricaoODataConsultaRepository
{
    IQueryable<PrescricaoODataDto> Consultar();
}
