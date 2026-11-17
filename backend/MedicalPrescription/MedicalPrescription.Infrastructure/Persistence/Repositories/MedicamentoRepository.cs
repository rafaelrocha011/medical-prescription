using MedicalPrescription.Application.Abstractions.Persistence;
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
            .FirstOrDefaultAsync(
                m => m.Id == id,
                cancellationToken);
    }
}
