
$ErrorActionPreference = "Stop"
$raiz = "C:\Projetos\prescricao-medica"

Set-Location $raiz

# Localizar projetos
$app = @(
    Get-ChildItem $raiz -Filter "MedicalPrescription.Application.csproj" -Recurse -File |
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
)

$tests = @(
    Get-ChildItem $raiz -Filter "MedicalPrescription.UnitTests.csproj" -Recurse -File |
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
)

if ($app.Count -ne 1 -or $tests.Count -ne 1) {
    throw "Nao foi possivel localizar Application e UnitTests."
}

function CriarArquivo {
    param(
        [string]$Base,
        [string]$Relativo,
        [string]$Conteudo
    )

    $destino = Join-Path $Base $Relativo
    $diretorio = Split-Path $destino -Parent

    New-Item -ItemType Directory -Force -Path $diretorio | Out-Null

    if (Test-Path $destino) {
        $atual = Get-Content $destino -Raw

        if (-not [string]::IsNullOrWhiteSpace($atual)) {
            Write-Host "JA EXISTE: $Relativo" -ForegroundColor Yellow
            return
        }
    }

    Set-Content -Path $destino -Value $Conteudo -Encoding UTF8

    Write-Host "CRIADO: $Relativo" -ForegroundColor Green
}

$base = $app[0].DirectoryName

# DTO para a listagem
CriarArquivo $base "DTOs\Prescricoes\PrescricaoResumoDto.cs" @'
using MedicalPrescription.Domain.Enums;

namespace MedicalPrescription.Application.DTOs.Prescricoes;

public sealed record PrescricaoResumoDto(
    Guid Id,
    Guid PacienteId,
    TipoPrescricao Tipo,
    DateTime CriadaEmUtc,
    int QuantidadeMedicamentos
);
'@

# DTO generico de paginacao
CriarArquivo $base "DTOs\Prescricoes\ResultadoPaginado.cs" @'
namespace MedicalPrescription.Application.DTOs.Prescricoes;

public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalRegistros)
{
    public int TotalPaginas =>
        TamanhoPagina > 0
            ? (int)Math.Ceiling(
                (double)TotalRegistros / TamanhoPagina)
            : 0;
}
'@

# Contrato de consulta para a Infrastructure
CriarArquivo $base "Abstractions\Persistence\IPrescricaoConsultaRepository.cs" @'
using MedicalPrescription.Application.DTOs.Prescricoes;

namespace MedicalPrescription.Application.Abstractions.Persistence;

public interface IPrescricaoConsultaRepository
{
    Task<ResultadoPaginado<PrescricaoResumoDto>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default);
}
'@

# Interface do Use Case
CriarArquivo $base "Abstractions\UseCases\Prescricoes\IListarPrescricoesUseCase.cs" @'
using MedicalPrescription.Application.DTOs.Prescricoes;

namespace MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;

public interface IListarPrescricoesUseCase
{
    Task<ResultadoPaginado<PrescricaoResumoDto>> ExecutarAsync(
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default);
}
'@

# Implementacao do Use Case
CriarArquivo $base "UseCases\Prescricoes\ListarPrescricoes\ListarPrescricoesUseCase.cs" @'
using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;
using MedicalPrescription.Application.DTOs.Prescricoes;

namespace MedicalPrescription.Application.UseCases.Prescricoes.ListarPrescricoes;

public sealed class ListarPrescricoesUseCase
    : IListarPrescricoesUseCase
{
    private const int TamanhoMaximoPagina = 100;

    private readonly IPrescricaoConsultaRepository _repository;

    public ListarPrescricoesUseCase(
        IPrescricaoConsultaRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultadoPaginado<PrescricaoResumoDto>> ExecutarAsync(
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        if (pagina < 1)
            throw new ArgumentOutOfRangeException(
                nameof(pagina),
                "A pagina deve ser maior ou igual a 1.");

        if (tamanhoPagina < 1 || tamanhoPagina > TamanhoMaximoPagina)
            throw new ArgumentOutOfRangeException(
                nameof(tamanhoPagina),
                "O tamanho da pagina deve estar entre 1 e 100.");

        return await _repository.ListarAsync(
            pagina,
            tamanhoPagina,
            cancellationToken);
    }
}
'@

# Testes unitarios
CriarArquivo $tests[0].DirectoryName "UseCases\Prescricoes\ListarPrescricoesUseCaseTests.cs" @'
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
'@

Write-Host ""
Write-Host "Executando testes..." -ForegroundColor Cyan

dotnet test $tests[0].FullName

if ($LASTEXITCODE -ne 0) {
    throw "Falha na execucao dos testes."
}

Write-Host ""
Write-Host "Use Case de listagem implementado com sucesso!" -ForegroundColor Green
