using MedicalPrescription.Application.DTOs.Prescricoes;
using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.Queries.Prescricoes.ObterPrescricao;
using MedicalPrescription.Application.Commands.Prescricoes.ExcluirPrescricao;
using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Domain.Enums;
using Xunit;

namespace MedicalPrescription.UnitTests.UseCases.Prescricoes;

public class ConsultarExcluirPrescricaoTests
{
    [Fact]
    public async Task ObterNaoEstruturada_DeveRetornarDetalhes()
    {
        var repo = new FakeRepository();
        var prescricao = Prescricao.CriarNaoEstruturada(
            Guid.NewGuid(), "Receita em texto livre");

        await repo.AdicionarAsync(prescricao);

        var useCase = new ObterPrescricaoUseCase(new FakeReadRepository(repo));
        var resultado = await useCase.ExecutarAsync(prescricao.Id);

        Assert.Equal(prescricao.Id, resultado.Id);
        Assert.Equal(TipoPrescricao.NaoEstruturada, resultado.Tipo);
        Assert.Equal("Receita em texto livre", resultado.TextoLivre);
        Assert.Empty(resultado.Itens);
    }

    [Fact]
    public async Task ObterEstruturada_DeveRetornarMedicamentoEDosagem()
    {
        var repo = new FakeRepository();

        var item = new PrescricaoItem(
            medicamentoId: null,
            nomeMedicamento: "Medicamento manipulado",
            formaFarmaceutica: "Comprimido",
            dose: 500,
            unidadeMedida: "mg",
            frequencia: "A cada 8 horas",
            inicioTratamento: new DateOnly(2026, 11, 17),
            duracaoTratamentoDias: 7,
            usoContinuo: false,
            viaAdministracao: "Oral",
            prazoUtilizacao: "7 dias");

        var prescricao = Prescricao.CriarEstruturada(
            Guid.NewGuid(), new[] { item });

        await repo.AdicionarAsync(prescricao);

        var resultado = await new ObterPrescricaoUseCase(new FakeReadRepository(repo))
            .ExecutarAsync(prescricao.Id);

        var medicamento = Assert.Single(resultado.Itens);

        Assert.Equal(500, medicamento.Dose);
        Assert.Equal("mg", medicamento.UnidadeMedida);
        Assert.Equal("Medicamento manipulado", medicamento.NomeMedicamento);
    }

    [Fact]
    public async Task ObterInexistente_DeveLancarExcecao()
    {
        var useCase = new ObterPrescricaoUseCase(new FakeReadRepository(new FakeRepository()));

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => useCase.ExecutarAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ExcluirExistente_DeveRemoverESalvar()
    {
        var repo = new FakeRepository();
        var unitOfWork = new FakeUnitOfWork();

        var prescricao = Prescricao.CriarNaoEstruturada(
            Guid.NewGuid(), "Texto");

        await repo.AdicionarAsync(prescricao);

        var useCase = new ExcluirPrescricaoUseCase(repo, unitOfWork);
        await useCase.ExecutarAsync(prescricao.Id);

        Assert.Null(await repo.ObterParaEscritaAsync(prescricao.Id));
        Assert.Equal(1, unitOfWork.Chamadas);
    }

    [Fact]
    public async Task ExcluirInexistente_NaoDeveSalvar()
    {
        var repo = new FakeRepository();
        var unitOfWork = new FakeUnitOfWork();

        var useCase = new ExcluirPrescricaoUseCase(repo, unitOfWork);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => useCase.ExecutarAsync(Guid.NewGuid()));

        Assert.Equal(0, unitOfWork.Chamadas);
    }

    private sealed class FakeRepository : IPrescricaoWriteRepository
    {
        private readonly Dictionary<Guid, Prescricao> _dados = new();

        public Task<Prescricao?> ObterParaEscritaAsync(
            Guid id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_dados.GetValueOrDefault(id));

        public Task AdicionarAsync(
            Prescricao prescricao,
            CancellationToken cancellationToken = default)
        {
            _dados[prescricao.Id] = prescricao;
            return Task.CompletedTask;
        }

        public void Remover(Prescricao prescricao)
            => _dados.Remove(prescricao.Id);
    }

    private sealed class FakeReadRepository : IPrescricaoReadRepository
    {
        private readonly FakeRepository _write;

        public FakeReadRepository(FakeRepository write)
        {
            _write = write;
        }

        public async Task<PrescricaoDetalheDto?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var p = await _write.ObterParaEscritaAsync(
                id, cancellationToken);

            if (p is null)
                return null;

            var itens = p.Itens.Select(i => new PrescricaoItemDto(
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
                i.Observacao)).ToArray();

            return new PrescricaoDetalheDto(
                p.Id,
                p.PacienteId,
                p.Tipo,
                p.TextoLivre,
                p.CriadaEmUtc, itens, "Paciente teste");
        }

        public Task<ResultadoPaginado<PrescricaoResumoDto>> ListarAsync(
            int pagina,
            int tamanhoPagina,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "Este fake exercita somente a consulta de detalhes.");
        }
    }
    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int Chamadas { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            Chamadas++;
            return Task.FromResult(1);
        }
    }
}
