using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.DTOs.Prescricoes;
using MedicalPrescription.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.Infrastructure.Persistence.Repositories;

public sealed class PrescricaoConsultaRepository
    : IPrescricaoConsultaRepository
{
    private readonly MedicalPrescriptionDbContext _context;

    public PrescricaoConsultaRepository(
        MedicalPrescriptionDbContext context)
    {
        _context = context;
    }

    public async Task<ResultadoPaginado<PrescricaoResumoDto>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Prescricoes.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(p => p.CriadaEmUtc)
            .ThenBy(p => p.Id)
            .Skip(checked((pagina - 1) * tamanhoPagina))
            .Take(tamanhoPagina)
            .Select(p => new PrescricaoResumoDto(
                p.Id,
                p.PacienteId,
                p.Tipo,
                p.CriadaEmUtc,
                p.Itens.Count))
            .ToListAsync(cancellationToken);

        return new ResultadoPaginado<PrescricaoResumoDto>(
            itens,
            pagina,
            tamanhoPagina,
            total);
    }
}
