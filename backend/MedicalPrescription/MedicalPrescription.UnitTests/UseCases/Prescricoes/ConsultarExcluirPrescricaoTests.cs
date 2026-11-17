using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.UseCases.Prescricoes.ObterPrescricao;
using MedicalPrescription.Application.UseCases.Prescricoes.ExcluirPrescricao;
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

        var useCase = new ObterPrescricaoUseCase(repo);
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

        var resultado = await new ObterPrescricaoUseCase(repo)
            .ExecutarAsync(prescricao.Id);

        var medicamento = Assert.Single(resultado.Itens);

        Assert.Equal(500, medicamento.Dose);
        Assert.Equal("mg", medicamento.UnidadeMedida);
        Assert.Equal("Medicamento manipulado", medicamento.NomeMedicamento);
    }

    [Fact]
    public async Task ObterInexistente_DeveLancarExcecao()
    {
        var useCase = new ObterPrescricaoUseCase(new FakeRepository());

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

        Assert.Null(await repo.ObterPorIdAsync(prescricao.Id));
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

    private sealed class FakeRepository : IPrescricaoRepository
    {
        private readonly Dictionary<Guid, Prescricao> _dados = new();

        public Task<Prescricao?> ObterPorIdAsync(
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
