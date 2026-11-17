using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.DTOs.Prescricoes;
using MedicalPrescription.Application.UseCases.Prescricoes.ListarPrescricoes;
using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Domain.Enums;
using Xunit;

namespace MedicalPrescription.UnitTests.UseCases.Prescricoes;

public class ListarPrescricoesUseCaseTests
{
    private readonly FakeConsultaRepository _repository = new();
    private readonly ListarPrescricoesUseCase _useCase;

    public ListarPrescricoesUseCaseTests()
    {
        _useCase = new ListarPrescricoesUseCase(_repository);
    }

    [Fact]
    public async Task Listar_PrimeiraPagina_DeveRetornarDadosPaginados()
    {
        AdicionarPrescricoes(3);

        var resultado = await _useCase.ExecutarAsync(1, 2);

        Assert.Equal(2, resultado.Itens.Count);
        Assert.Equal(3, resultado.TotalRegistros);
        Assert.Equal(2, resultado.TotalPaginas);
        Assert.Equal(1, resultado.Pagina);
        Assert.Equal(1, _repository.Chamadas);
    }

    [Fact]
    public async Task Listar_SegundaPagina_DeveRetornarRegistroRestante()
    {
        AdicionarPrescricoes(3);

        var resultado = await _useCase.ExecutarAsync(2, 2);

        Assert.Single(resultado.Itens);
        Assert.Equal(3, resultado.TotalRegistros);
        Assert.Equal(2, resultado.TotalPaginas);
        Assert.Equal(2, resultado.Pagina);
    }

    [Fact]
    public async Task Listar_SemRegistros_DeveRetornarListaVazia()
    {
        var resultado = await _useCase.ExecutarAsync();

        Assert.Empty(resultado.Itens);
        Assert.Equal(0, resultado.TotalRegistros);
        Assert.Equal(0, resultado.TotalPaginas);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task Listar_ParametrosInvalidos_DeveRejeitar(
        int pagina,
        int tamanhoPagina)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _useCase.ExecutarAsync(pagina, tamanhoPagina));

        Assert.Equal(0, _repository.Chamadas);
    }

    [Fact]
    public async Task Listar_Estruturada_DeveInformarQuantidadeMedicamentos()
    {
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
            Guid.NewGuid(),
            new[] { item });

        _repository.Adicionar(prescricao);

        var resultado = await _useCase.ExecutarAsync();

        var resumo = Assert.Single(resultado.Itens);

        Assert.Equal(TipoPrescricao.Estruturada, resumo.Tipo);
        Assert.Equal(1, resumo.QuantidadeMedicamentos);
    }

    private void AdicionarPrescricoes(int quantidade)
    {
        for (var i = 0; i < quantidade; i++)
        {
            _repository.Adicionar(
                Prescricao.CriarNaoEstruturada(
                    Guid.NewGuid(),
                    $"Prescricao {i + 1}"));
        }
    }

    private sealed class FakeConsultaRepository
        : IPrescricaoConsultaRepository
    {
        private readonly List<Prescricao> _dados = new();

        public int Chamadas { get; private set; }

        public void Adicionar(Prescricao prescricao)
        {
            _dados.Add(prescricao);
        }

        public Task<ResultadoPaginado<PrescricaoResumoDto>> ListarAsync(
            int pagina,
            int tamanhoPagina,
            CancellationToken cancellationToken = default)
        {
            Chamadas++;

            var total = _dados.Count;

            var itens = _dados
                .OrderByDescending(x => x.CriadaEmUtc)
                .ThenBy(x => x.Id)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .Select(x => new PrescricaoResumoDto(
                    x.Id,
                    x.PacienteId,
                    x.Tipo,
                    x.CriadaEmUtc,
                    x.Itens.Count))
                .ToArray();

            var resultado = new ResultadoPaginado<PrescricaoResumoDto>(
                itens,
                pagina,
                tamanhoPagina,
                total);

            return Task.FromResult(resultado);
        }
    }
}
