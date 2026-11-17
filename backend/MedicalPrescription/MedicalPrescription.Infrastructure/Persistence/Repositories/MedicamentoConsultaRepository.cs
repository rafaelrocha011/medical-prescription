using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.DTOs.Catalogos;
using MedicalPrescription.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.Infrastructure.Persistence.Repositories;

public sealed class MedicamentoConsultaRepository : IMedicamentoConsultaRepository
{
    private readonly MedicalPrescriptionDbContext _context;

    public MedicamentoConsultaRepository(MedicalPrescriptionDbContext context)
    {
        _context = context;
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
            .Select(m => new MedicamentoResumoDto(
                m.Id,
                m.Nome))
            .ToListAsync(cancellationToken);
    }
}
