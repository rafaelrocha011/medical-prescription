using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.DTOs.Catalogos;
using MedicalPrescription.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.Infrastructure.Persistence.Repositories;

public sealed class PacienteConsultaRepository : IPacienteConsultaRepository
{
    private readonly MedicalPrescriptionDbContext _context;

    public PacienteConsultaRepository(MedicalPrescriptionDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PacienteResumoDto>> ListarAsync(
        string? busca,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Pacientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(p => p.NomeCompleto.Contains(busca));

        return await query
            .OrderBy(p => p.NomeCompleto)
            .ThenBy(p => p.Id)
            .Take(50)
            .Select(p => new PacienteResumoDto(
                p.Id,
                p.NomeCompleto))
            .ToListAsync(cancellationToken);
    }
}
