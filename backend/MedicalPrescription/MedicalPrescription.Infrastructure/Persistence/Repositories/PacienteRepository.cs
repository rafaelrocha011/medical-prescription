using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.DTOs.Catalogos;
using MedicalPrescription.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.Infrastructure.Persistence.Repositories;

public sealed class PacienteRepository : IPacienteRepository
{
    private readonly MedicalPrescriptionDbContext _context;

    public PacienteRepository(MedicalPrescriptionDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExisteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.Pacientes
            .AsNoTracking()
            .AnyAsync(p => p.Id == id, cancellationToken);
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
            .Select(p => new PacienteResumoDto(p.Id, p.NomeCompleto))
            .ToListAsync(cancellationToken);
    }
}