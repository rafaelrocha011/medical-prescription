
$ErrorActionPreference = "Stop"
$raiz = "C:\Projetos\prescricao-medica"
Set-Location $raiz

# ===========================================
# 1. GITFLOW
# ===========================================

$branch = (git branch --show-current).Trim()

if ($branch -ne "feature/infra-mysql") {

    if (git status --porcelain) {
        throw "Existem alteracoes locais. Salve-as antes de trocar de branch."
    }

    git switch develop
    if ($LASTEXITCODE -ne 0) { throw "Falha ao acessar develop." }

    git pull --ff-only origin develop
    if ($LASTEXITCODE -ne 0) { throw "Falha ao atualizar develop." }

    $existe = git branch --list "feature/infra-mysql"

    if ($existe) {
        git switch feature/infra-mysql
    }
    else {
        git switch -c feature/infra-mysql
    }

    if ($LASTEXITCODE -ne 0) { throw "Falha ao acessar feature." }
}

# ===========================================
# 2. LOCALIZAR OS PROJETOS
# ===========================================

function EncontrarProjeto([string]$nome) {
    $arquivos = @(
        Get-ChildItem $raiz -Filter "$nome.csproj" -Recurse -File |
        Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
    )

    if ($arquivos.Count -ne 1) {
        throw "Projeto nao encontrado de forma unica: $nome"
    }

    return $arquivos[0]
}

$infra = EncontrarProjeto "MedicalPrescription.Infrastructure"
$api = EncontrarProjeto "MedicalPrescription.Api"
$tests = EncontrarProjeto "MedicalPrescription.UnitTests"

$base = $infra.DirectoryName

function CriarArquivo {
    param(
        [string]$Caminho,
        [string]$Conteudo
    )

    $pasta = Split-Path $Caminho -Parent
    New-Item -ItemType Directory -Path $pasta -Force | Out-Null

    if (Test-Path $Caminho) {
        $atual = Get-Content $Caminho -Raw

        if (-not [string]::IsNullOrWhiteSpace($atual)) {
            Write-Host "PRESERVADO: $Caminho" -ForegroundColor Yellow
            return
        }
    }

    Set-Content -Path $Caminho -Value $Conteudo -Encoding UTF8
    Write-Host "CRIADO: $Caminho" -ForegroundColor Green
}

# ===========================================
# 3. INSTALAR PACOTES
# ===========================================

dotnet add $infra.FullName package Pomelo.EntityFrameworkCore.MySql --version 9.0.0
if ($LASTEXITCODE -ne 0) { throw "Falha ao instalar Pomelo." }

dotnet add $api.FullName package Microsoft.EntityFrameworkCore.Design --version 9.0.23
if ($LASTEXITCODE -ne 0) { throw "Falha ao instalar EF Design." }

# ===========================================
# 4. DB CONTEXT + UNIT OF WORK
# ===========================================

CriarArquivo (Join-Path $base "Persistence\Context\MedicalPrescriptionDbContext.cs") @'
using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.Infrastructure.Persistence.Context;

public sealed class MedicalPrescriptionDbContext
    : DbContext, IUnitOfWork
{
    public MedicalPrescriptionDbContext(
        DbContextOptions<MedicalPrescriptionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Medicamento> Medicamentos => Set<Medicamento>();
    public DbSet<Prescricao> Prescricoes => Set<Prescricao>();
    public DbSet<PrescricaoItem> PrescricaoItens => Set<PrescricaoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MedicalPrescriptionDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
'@

# ===========================================
# 5. ENTITY CONFIGURATIONS
# ===========================================

CriarArquivo (Join-Path $base "Persistence\Configurations\PacienteConfiguration.cs") @'
using MedicalPrescription.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalPrescription.Infrastructure.Persistence.Configurations;

public sealed class PacienteConfiguration
    : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("pacientes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.NomeCompleto)
            .HasColumnName("nome_completo")
            .HasMaxLength(200)
            .IsRequired();
    }
}
'@

CriarArquivo (Join-Path $base "Persistence\Configurations\MedicamentoConfiguration.cs") @'
using MedicalPrescription.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalPrescription.Infrastructure.Persistence.Configurations;

public sealed class MedicamentoConfiguration
    : IEntityTypeConfiguration<Medicamento>
{
    public void Configure(EntityTypeBuilder<Medicamento> builder)
    {
        builder.ToTable("medicamentos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.Nome)
            .HasColumnName("nome")
            .HasMaxLength(200)
            .IsRequired();
    }
}
'@

CriarArquivo (Join-Path $base "Persistence\Configurations\PrescricaoConfiguration.cs") @'
using MedicalPrescription.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalPrescription.Infrastructure.Persistence.Configurations;

public sealed class PrescricaoConfiguration
    : IEntityTypeConfiguration<Prescricao>
{
    public void Configure(EntityTypeBuilder<Prescricao> builder)
    {
        builder.ToTable("prescricoes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.PacienteId)
            .HasColumnName("paciente_id")
            .IsRequired();

        builder.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.TextoLivre)
            .HasColumnName("texto_livre")
            .HasColumnType("longtext");

        builder.Property(x => x.CriadaEmUtc)
            .HasColumnName("criada_em_utc")
            .HasColumnType("datetime(6)")
            .IsRequired();

        builder.HasOne<Paciente>()
            .WithMany()
            .HasForeignKey(x => x.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Itens)
            .WithOne()
            .HasForeignKey(x => x.PrescricaoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Itens)
            .HasField("_itens")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.CriadaEmUtc);
    }
}
'@

CriarArquivo (Join-Path $base "Persistence\Configurations\PrescricaoItemConfiguration.cs") @'
using MedicalPrescription.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalPrescription.Infrastructure.Persistence.Configurations;

public sealed class PrescricaoItemConfiguration
    : IEntityTypeConfiguration<PrescricaoItem>
{
    public void Configure(EntityTypeBuilder<PrescricaoItem> builder)
    {
        builder.ToTable("prescricao_itens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.PrescricaoId)
            .HasColumnName("prescricao_id")
            .IsRequired();

        builder.Property(x => x.MedicamentoId)
            .HasColumnName("medicamento_id");

        builder.HasOne<Medicamento>()
            .WithMany()
            .HasForeignKey(x => x.MedicamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.NomeMedicamento)
            .HasColumnName("nome_medicamento")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.FormaFarmaceutica)
            .HasColumnName("forma_farmaceutica")
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(x => x.Dosagem, dosagem =>
        {
            dosagem.Property(x => x.Quantidade)
                .HasColumnName("dose")
                .HasPrecision(12, 3)
                .IsRequired();

            dosagem.Property(x => x.UnidadeMedida)
                .HasColumnName("unidade_medida")
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.Navigation(x => x.Dosagem).IsRequired();

        builder.Property(x => x.Frequencia)
            .HasColumnName("frequencia")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.InicioTratamento)
            .HasColumnName("inicio_tratamento")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.DuracaoTratamentoDias)
            .HasColumnName("duracao_tratamento_dias");

        builder.Property(x => x.UsoContinuo)
            .HasColumnName("uso_continuo")
            .IsRequired();

        builder.Property(x => x.ViaAdministracao)
            .HasColumnName("via_administracao")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PrazoUtilizacao)
            .HasColumnName("prazo_utilizacao")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Observacao)
            .HasColumnName("observacao")
            .HasColumnType("longtext");
    }
}
'@

# ===========================================
# 6. REPOSITORIES
# ===========================================

CriarArquivo (Join-Path $base "Persistence\Repositories\PacienteRepository.cs") @'
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
'@

CriarArquivo (Join-Path $base "Persistence\Repositories\MedicamentoRepository.cs") @'
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
'@

CriarArquivo (Join-Path $base "Persistence\Repositories\PrescricaoRepository.cs") @'
using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.Infrastructure.Persistence.Repositories;

public sealed class PrescricaoRepository : IPrescricaoRepository
{
    private readonly MedicalPrescriptionDbContext _context;

    public PrescricaoRepository(MedicalPrescriptionDbContext context)
    {
        _context = context;
    }

    public Task<Prescricao?> ObterPorIdAsync(
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
'@

CriarArquivo (Join-Path $base "Persistence\Repositories\PrescricaoConsultaRepository.cs") @'
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
'@

# ===========================================
# 7. DEPENDENCY INJECTION
# ===========================================

CriarArquivo (Join-Path $base "DependencyInjection.cs") @'
using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Infrastructure.Persistence.Context;
using MedicalPrescription.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedicalPrescription.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("MySql");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:MySql nao foi configurada.");
        }

        services.AddDbContext<MedicalPrescriptionDbContext>(
            options => options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0))));

        services.AddScoped<IUnitOfWork>(
            provider => provider.GetRequiredService<
                MedicalPrescriptionDbContext>());

        services.AddScoped<
            IPacienteRepository,
            PacienteRepository>();

        services.AddScoped<
            IMedicamentoRepository,
            MedicamentoRepository>();

        services.AddScoped<
            IPrescricaoRepository,
            PrescricaoRepository>();

        services.AddScoped<
            IPrescricaoConsultaRepository,
            PrescricaoConsultaRepository>();

        return services;
    }
}
'@

# ===========================================
# 8. CONFIGURACAO DO MYSQL
# ===========================================

$gitignore = Join-Path $raiz ".gitignore"

if (-not (Test-Path $gitignore)) {
    throw "Arquivo .gitignore nao encontrado."
}

git check-ignore -q .env

if ($LASTEXITCODE -ne 0) {
    throw "O .env nao esta protegido pelo .gitignore."
}

CriarArquivo (Join-Path $raiz ".env.example") @'
MYSQL_ROOT_PASSWORD=substituir_por_senha_local
MYSQL_DATABASE=medical_prescription
MYSQL_USER=medical_user
MYSQL_PASSWORD=substituir_por_senha_local
MYSQL_PORT=3307
'@

function NovaSenha {
    $bytes = New-Object byte[] 24
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()

    try {
        $rng.GetBytes($bytes)
    }
    finally {
        $rng.Dispose()
    }

    return ([BitConverter]::ToString($bytes)).Replace("-", "")
}

$envLocal = Join-Path $raiz ".env"

if (-not (Test-Path $envLocal)) {
    $senhaRoot = NovaSenha
    $senhaApp = NovaSenha

    $conteudoEnv = @"
MYSQL_ROOT_PASSWORD=$senhaRoot
MYSQL_DATABASE=medical_prescription
MYSQL_USER=medical_user
MYSQL_PASSWORD=$senhaApp
MYSQL_PORT=3307
"@

    Set-Content $envLocal $conteudoEnv -Encoding UTF8
    Write-Host "CRIADO: .env com senhas locais geradas" -ForegroundColor Green
}

CriarArquivo (Join-Path $raiz "docker-compose.yml") @'
services:
  mysql:
    image: mysql:8.4
    restart: unless-stopped

    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD:?Obrigatorio}
      MYSQL_DATABASE: ${MYSQL_DATABASE:?Obrigatorio}
      MYSQL_USER: ${MYSQL_USER:?Obrigatorio}
      MYSQL_PASSWORD: ${MYSQL_PASSWORD:?Obrigatorio}

    ports:
      - "127.0.0.1:${MYSQL_PORT:-3307}:3306"

    volumes:
      - mysql_data:/var/lib/mysql

    healthcheck:
      test:
        - CMD-SHELL
        - mysqladmin ping -h 127.0.0.1 -u root -p"$$MYSQL_ROOT_PASSWORD" --silent
      interval: 10s
      timeout: 5s
      retries: 15
      start_period: 30s

volumes:
  mysql_data:
'@

# ===========================================
# 9. BUILD E TESTES
# ===========================================

Write-Host "`nCompilando Infrastructure..." -ForegroundColor Cyan

dotnet build $infra.FullName
if ($LASTEXITCODE -ne 0) {
    throw "Falha na compilacao da Infrastructure."
}

Write-Host "`nExecutando testes..." -ForegroundColor Cyan

dotnet test $tests.FullName
if ($LASTEXITCODE -ne 0) {
    throw "Falha na execucao dos testes."
}

# ===========================================
# 10. INICIAR MYSQL
# ===========================================

Write-Host "`nIniciando MySQL no Docker..." -ForegroundColor Cyan

docker compose up -d mysql
if ($LASTEXITCODE -ne 0) {
    throw "Falha ao iniciar o MySQL."
}

docker compose ps

Write-Host "`nInfrastructure criada e validada!" -ForegroundColor Green
