using MedicalPrescription.Application.Abstractions.Persistence;
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
}
