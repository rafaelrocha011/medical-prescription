using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.DTOs.Catalogos;
using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.Infrastructure.Persistence.Repositories;

public sealed class MedicamentoRepository : IMedicamentoRepository
{
    private readonly MedicalPrescriptionDbContext _context;

    public MedicamentoRepository(MedicalPrescriptionDbContext context)
    {
        _context = context;
    }

    public Task<Medicamento?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.Medicamentos
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MedicamentoResumoDto>> ListarAsync(
        string? busca,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Medicamentos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(m => m.Nome.Contains(busca));

        return await query
            .OrderBy(m => m.Nome)
            .ThenBy(m => m.Id)
            .Take(50)
            .Select(m => new MedicamentoResumoDto(m.Id, m.Nome))
            .ToListAsync(cancellationToken);
    }
}