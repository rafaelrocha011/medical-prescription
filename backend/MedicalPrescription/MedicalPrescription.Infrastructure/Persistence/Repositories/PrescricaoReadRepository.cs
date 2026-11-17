using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.DTOs.Prescricoes;
using MedicalPrescription.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.Infrastructure.Persistence.Repositories;

public sealed class PrescricaoReadRepository : IPrescricaoReadRepository, IPrescricaoODataConsultaRepository
{
    private readonly MedicalPrescriptionDbContext _context;

    public PrescricaoReadRepository(
        MedicalPrescriptionDbContext context)
    {
        _context = context;
    }

    public async Task<PrescricaoDetalheDto?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dados = await _context.Prescricoes
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.PacienteId,
                p.Tipo,
                p.TextoLivre,
                p.CriadaEmUtc,

                NomePaciente = _context.Pacientes

                    .Where(paciente => paciente.Id == p.PacienteId)

                    .Select(paciente => paciente.NomeCompleto)

                    .FirstOrDefault()!
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dados is null)
            return null;

        var itens = await _context
            .Set<MedicalPrescription.Domain.Entities.PrescricaoItem>()
            .AsNoTracking()
            .Where(i => i.PrescricaoId == id)
            .OrderBy(i => i.Id)
            .Select(i => new PrescricaoItemDto(
                i.Id,
                i.MedicamentoId,
                i.NomeMedicamento,
                i.FormaFarmaceutica,
                i.Dosagem.Quantidade,
                i.Dosagem.UnidadeMedida,
                i.Frequencia,
                i.InicioTratamento,
                i.DuracaoTratamentoDias,
                i.UsoContinuo,
                i.ViaAdministracao,
                i.PrazoUtilizacao,
                i.Observacao))
            .ToListAsync(cancellationToken);

        return new PrescricaoDetalheDto(
            dados.Id,
            dados.PacienteId,
            dados.Tipo,
            dados.TextoLivre,
            dados.CriadaEmUtc,
            itens,
            dados.NomePaciente);
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
                p.Itens.Count,

                _context.Pacientes

                    .Where(paciente => paciente.Id == p.PacienteId)

                    .Select(paciente => paciente.NomeCompleto)

                    .FirstOrDefault()!))
            .ToListAsync(cancellationToken);

        return new ResultadoPaginado<PrescricaoResumoDto>(
            itens,
            pagina,
            tamanhoPagina,
            total);
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
