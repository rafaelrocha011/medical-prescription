using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.DTOs.Prescricoes;
using MedicalPrescription.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.Infrastructure.Persistence.Repositories;

public sealed class PrescricaoODataConsultaRepository
    : IPrescricaoODataConsultaRepository
{
    private readonly MedicalPrescriptionDbContext _context;

    public PrescricaoODataConsultaRepository(
        MedicalPrescriptionDbContext context)
    {
        _context = context;
    }

    public IQueryable<PrescricaoODataDto> Consultar()
    {
        return _context.Prescricoes
            .AsNoTracking()
            .Select(p => new PrescricaoODataDto
            {
                Id = p.Id,
                PacienteId = p.PacienteId,
                Tipo = (int)p.Tipo,
                CriadaEmUtc = p.CriadaEmUtc,
                QuantidadeMedicamentos = p.Itens.Count
            });
    }
}
