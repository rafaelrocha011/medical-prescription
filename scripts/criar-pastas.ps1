
$ErrorActionPreference = "Stop"

$raiz = "C:\Projetos\prescricao-medica"
$base = Join-Path $raiz "backend\MedicalPrescription"
$nome = "MedicalPrescription.IntegrationTests"
$pasta = Join-Path $base $nome
$projeto = Join-Path $pasta "$nome.csproj"
$solucao = Join-Path $base "MedicalPrescription.slnx"
$unitTests = Join-Path $base "MedicalPrescription.UnitTests\MedicalPrescription.UnitTests.csproj"

$utf8 = New-Object System.Text.UTF8Encoding($false)

Set-Location $raiz

if ((git branch --show-current).Trim() -ne "refactor/backend") {
    throw "Execute na branch refactor/backend."
}

if (-not (Test-Path $solucao)) {
    throw "Solucao nao encontrada."
}

# Validar Docker Desktop
docker info --format '{{.ServerVersion}}'

if ($LASTEXITCODE -ne 0) {
    throw "Docker Desktop nao esta disponivel."
}

function Salvar($relativo, $conteudo) {
    $destino = Join-Path $pasta $relativo

    New-Item -ItemType Directory -Force `
        -Path (Split-Path $destino -Parent) | Out-Null

    [IO.File]::WriteAllText($destino, $conteudo, $utf8)

    Write-Host "CRIADO: $relativo" -ForegroundColor Green
}

# Permitir nova execucao sem sobrescrever os testes.
if (Test-Path $pasta) {
    $esperados = @(
        $projeto,
        (Join-Path $pasta "MySqlFixture.cs"),
        (Join-Path $pasta "PrescricaoPersistenciaTests.cs")
    )

    foreach ($arquivo in $esperados) {
        if (-not (Test-Path $arquivo)) {
            throw "Projeto parcialmente criado: $arquivo"
        }
    }

    Write-Host "Projeto de integracao ja existe. Preservando arquivos." `
        -ForegroundColor Yellow
}
else {

    New-Item -ItemType Directory -Force -Path $pasta | Out-Null

    # ================================================
    # 1. PROJETO DE TESTES
    # ================================================

    Salvar "$nome.csproj" @'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="Testcontainers.MySql" Version="4.16.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MedicalPrescription.Infrastructure\MedicalPrescription.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

</Project>
'@

    # ================================================
    # 2. FIXTURE MYSQL
    # ================================================

    Salvar "MySqlFixture.cs" @'
using MedicalPrescription.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;

namespace MedicalPrescription.IntegrationTests;

public sealed class MySqlFixture : IAsyncLifetime
{
    private readonly MySqlContainer _container =
        new MySqlBuilder("mysql:8.4").Build();

    public MedicalPrescriptionDbContext NovoContexto()
    {
        var options = new DbContextOptionsBuilder<MedicalPrescriptionDbContext>()
            .UseMySql(
                _container.GetConnectionString(),
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;

        return new MedicalPrescriptionDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = NovoContexto();

        // Aplica as migrations reais no banco temporario.
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
'@

    # ================================================
    # 3. TESTES DE PERSISTENCIA
    # ================================================

    Salvar "PrescricaoPersistenciaTests.cs" @'
using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.IntegrationTests;

public sealed class PrescricaoPersistenciaTests
    : IClassFixture<MySqlFixture>
{
    private readonly MySqlFixture _fixture;

    public PrescricaoPersistenciaTests(MySqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SubstituirItens_DeveExcluirAntigosEPersistirNovos()
    {
        // Arrange: criar uma prescricao com um medicamento.
        var paciente = new Paciente("Paciente Integracao");

        var prescricao = Prescricao.CriarEstruturada(
            paciente.Id,
            new[] { NovoItem("Original", 100m) });

        var idOriginal = prescricao.Itens.Single().Id;

        await using (var context = _fixture.NovoContexto())
        {
            context.Pacientes.Add(paciente);

            var repository = new PrescricaoWriteRepository(context);

            await repository.AdicionarAsync(prescricao);
            await context.SaveChangesAsync();
        }

        // Act: carregar agregado rastreado e substituir os itens.
        await using (var context = _fixture.NovoContexto())
        {
            var repository = new PrescricaoWriteRepository(context);

            var agregado = await repository.ObterParaEscritaAsync(
                prescricao.Id);

            Assert.NotNull(agregado);

            agregado.SubstituirItens(new[]
            {
                NovoItem("Novo medicamento A", 250m),
                NovoItem("Novo medicamento B", 500m)
            });

            await context.SaveChangesAsync();
        }

        // Assert: consultar com novo DbContext para verificar o banco.
        await using (var context = _fixture.NovoContexto())
        {
            var itens = await context.PrescricaoItens
                .AsNoTracking()
                .Where(i => i.PrescricaoId == prescricao.Id)
                .ToListAsync();

            Assert.Equal(2, itens.Count);

            Assert.DoesNotContain(
                itens,
                item => item.Id == idOriginal);

            Assert.Contains(
                itens,
                item =>
                    item.NomeMedicamento == "Novo medicamento A" &&
                    item.Dosagem.Quantidade == 250m);

            Assert.Contains(
                itens,
                item =>
                    item.NomeMedicamento == "Novo medicamento B" &&
                    item.Dosagem.Quantidade == 500m);

            Assert.All(
                itens,
                item => Assert.Equal(prescricao.Id, item.PrescricaoId));
        }
    }

    [Fact]
    public async Task ExcluirPrescricao_DeveExcluirItensEPreservarPaciente()
    {
        // Arrange
        var paciente = new Paciente("Paciente Exclusao");

        var prescricao = Prescricao.CriarEstruturada(
            paciente.Id,
            new[] { NovoItem("Medicamento teste", 100m) });

        await using (var context = _fixture.NovoContexto())
        {
            context.Pacientes.Add(paciente);
            context.Prescricoes.Add(prescricao);

            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = _fixture.NovoContexto())
        {
            var repository = new PrescricaoWriteRepository(context);

            var agregado = await repository.ObterParaEscritaAsync(
                prescricao.Id);

            Assert.NotNull(agregado);

            repository.Remover(agregado);

            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = _fixture.NovoContexto())
        {
            Assert.False(await context.Prescricoes
                .AnyAsync(p => p.Id == prescricao.Id));

            Assert.False(await context.PrescricaoItens
                .AnyAsync(i => i.PrescricaoId == prescricao.Id));

            Assert.True(await context.Pacientes
                .AnyAsync(p => p.Id == paciente.Id));
        }
    }

    [Fact]
    public async Task ConsultarDetalhes_NaoDeveRastrearEntidades()
    {
        // Arrange
        var paciente = new Paciente("Paciente Consulta");

        var prescricao = Prescricao.CriarEstruturada(
            paciente.Id,
            new[] { NovoItem("Medicamento consulta", 300m) });

        await using (var context = _fixture.NovoContexto())
        {
            context.Pacientes.Add(paciente);
            context.Prescricoes.Add(prescricao);

            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = _fixture.NovoContexto())
        {
            var repository = new PrescricaoReadRepository(context);

            var resultado = await repository.ObterPorIdAsync(
                prescricao.Id);

            // Assert
            Assert.NotNull(resultado);

            Assert.Equal(prescricao.Id, resultado.Id);
            Assert.Equal(paciente.Id, resultado.PacienteId);

            var item = Assert.Single(resultado.Itens);

            Assert.Equal("Medicamento consulta", item.NomeMedicamento);
            Assert.Equal(300m, item.Dose);
            Assert.Equal("mg", item.UnidadeMedida);

            Assert.Empty(context.ChangeTracker.Entries());
        }
    }

    private static PrescricaoItem NovoItem(string nome, decimal dose)
    {
        return new PrescricaoItem(
            medicamentoId: null,
            nomeMedicamento: nome,
            formaFarmaceutica: "Comprimido",
            dose: dose,
            unidadeMedida: "mg",
            frequencia: "A cada 8 horas",
            inicioTratamento: new DateOnly(2026, 11, 17),
            duracaoTratamentoDias: 7,
            usoContinuo: false,
            viaAdministracao: "Oral",
            prazoUtilizacao: "7 dias",
            observacao: null);
    }
}
'@
}

# ================================================
# 4. ADICIONAR PROJETO A SOLUCAO
# ================================================

$conteudo = [IO.File]::ReadAllText($solucao)

if (-not $conteudo.Contains(
    "MedicalPrescription.IntegrationTests.csproj"
)) {

    $marcador = '<Project Path="MedicalPrescription.UnitTests/MedicalPrescription.UnitTests.csproj" />'

    if (-not $conteudo.Contains($marcador)) {
        throw "Ponto de insercao nao encontrado na solucao."
    }

    $backup = "$solucao.bak"

    Copy-Item $solucao $backup -Force

    $novoProjeto = '<Project Path="MedicalPrescription.IntegrationTests/MedicalPrescription.IntegrationTests.csproj" />'

    $conteudo = $conteudo.Replace(
        $marcador,
        $marcador + "`r`n    " + $novoProjeto
    )

    [IO.File]::WriteAllText($solucao, $conteudo, $utf8)

    Write-Host "Projeto adicionado a solucao." -ForegroundColor Green
}

# ================================================
# 5. RESTORE, BUILD E TESTES
# ================================================

Write-Host "`nCompilando solucao..." -ForegroundColor Cyan

dotnet build $solucao

if ($LASTEXITCODE -ne 0) {
    throw "Build falhou. Arquivos mantidos para diagnostico."
}

Write-Host "`nExecutando os 42 testes unitarios..." `
    -ForegroundColor Cyan

dotnet test $unitTests --no-build

if ($LASTEXITCODE -ne 0) {
    throw "Testes unitarios falharam."
}

Write-Host "`nExecutando integracao com MySQL real..." `
    -ForegroundColor Cyan

dotnet test $projeto --no-build --logger "console;verbosity=normal"

if ($LASTEXITCODE -ne 0) {
    throw "Testes de integracao falharam. Verifique o erro."
}

Write-Host "`nTODOS OS TESTES PASSARAM!" `
    -ForegroundColor Green

Write-Host "`nAlteracoes:" -ForegroundColor Cyan
git status --short
