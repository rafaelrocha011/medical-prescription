using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.Infrastructure.Persistence.Repositories;

public sealed class PrescricaoWriteRepository : IPrescricaoWriteRepository
{
    private readonly MedicalPrescriptionDbContext _context;

    public PrescricaoWriteRepository(MedicalPrescriptionDbContext context)
    {
        _context = context;
    }

    public Task<Prescricao?> ObterParaEscritaAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.Prescricoes
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(
                p => p.Id == id,
                cancellationToken);
    }

    public async Task AdicionarAsync(
        Prescricao prescricao,
        CancellationToken cancellationToken = default)
    {
        await _context.Prescricoes.AddAsync(
            prescricao,
            cancellationToken);
    }

    public void Remover(Prescricao prescricao)
    {
        _context.Prescricoes.Remove(prescricao);
    }
}
