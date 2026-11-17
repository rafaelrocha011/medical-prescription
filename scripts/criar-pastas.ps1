
$ErrorActionPreference = "Stop"
$raiz = "C:\Projetos\prescricao-medica"
Set-Location $raiz

function EncontrarProjeto($nome) {
    $projetos = @(
        Get-ChildItem $raiz -Filter "$nome.csproj" -Recurse -File |
        Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
    )

    if ($projetos.Count -ne 1) {
        throw "Projeto nao encontrado: $nome"
    }

    return $projetos[0].FullName
}

$api = EncontrarProjeto "MedicalPrescription.Api"
$infra = EncontrarProjeto "MedicalPrescription.Infrastructure"
$tests = EncontrarProjeto "MedicalPrescription.UnitTests"

$env:ASPNETCORE_ENVIRONMENT = "Development"

# Compilar
dotnet build $api
if ($LASTEXITCODE -ne 0) { throw "Falha no build." }

# Executar testes
dotnet test $tests
if ($LASTEXITCODE -ne 0) { throw "Falha nos testes." }

# Localizar migrations existentes
$pastaMigrations = Join-Path (Split-Path $infra -Parent) `
    "Persistence\Migrations"

$migrations = @()

if (Test-Path $pastaMigrations) {
    $migrations = @(
        Get-ChildItem $pastaMigrations -Filter "*_CriacaoInicial.cs" -File
    )
}

# Criar migration somente se nao existir
if ($migrations.Count -eq 0) {

    dotnet ef migrations add CriacaoInicial `
        --project $infra `
        --startup-project $api `
        --output-dir Persistence/Migrations

    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao criar migration."
    }
}

# Gerar script SQL
$pastaDatabase = Join-Path $raiz "database"

New-Item -ItemType Directory `
    -Path $pastaDatabase -Force | Out-Null

dotnet ef migrations script `
    --project $infra `
    --startup-project $api `
    --output (Join-Path $pastaDatabase "schema.sql")

if ($LASTEXITCODE -ne 0) {
    throw "Falha ao gerar script SQL."
}

# Aplicar migration no MySQL
dotnet ef database update `
    --project $infra `
    --startup-project $api

if ($LASTEXITCODE -ne 0) {
    throw "Falha ao atualizar o banco."
}

Write-Host "`nMigration aplicada com sucesso!" -ForegroundColor Green
