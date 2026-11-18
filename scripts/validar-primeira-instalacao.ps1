<#
.SYNOPSIS
  Simula a primeira instalacao com MySQL vazio, seed dos catalogos e reinicio da API.
.DESCRIPTION
  Usa um projeto Compose com nome exclusivo. Nao toca no projeto/volume padrao.
  Requer Docker Compose v2 e acesso ao Docker Engine.
.EXAMPLE
  ./scripts/validar-primeira-instalacao.ps1
.EXAMPLE
  ./scripts/validar-primeira-instalacao.ps1 -KeepEnvironment
#>
param(
    [int]$ApiPort = 15080,
    [int]$FrontendPort = 14200,
    [int]$MySqlPort = 13307,
    [switch]$KeepEnvironment
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$compose = Join-Path $root 'docker-compose.yml'
if (-not (Test-Path $compose)) { throw 'docker-compose.yml nao encontrado.' }
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw 'Docker nao encontrado no PATH.' }

$project = 'mpfresh-' + [DateTime]::UtcNow.ToString('yyyyMMddHHmmss') + '-' +
    [Guid]::NewGuid().ToString('N').Substring(0, 6)
$keys = @(
    'MYSQL_ROOT_PASSWORD', 'MYSQL_PASSWORD', 'MYSQL_DATABASE',
    'MYSQL_USER', 'MYSQL_PORT', 'API_PORT', 'FRONTEND_PORT'
)
$original = @{}
foreach ($key in $keys) {
    $original[$key] = [Environment]::GetEnvironmentVariable($key, 'Process')
}

function Invoke-Compose {
    param([string[]]$Arguments)
    & docker compose -p $project -f $compose @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Compose falhou: $($Arguments -join ' ')"
    }
}

function Test-Catalogos {
    param([int]$MaxTentativas = 1)
    for ($attempt = 1; $attempt -le $MaxTentativas; $attempt++) {
        try {
            $pacientes = @(Invoke-RestMethod -Uri "http://127.0.0.1:$ApiPort/api/pacientes" -TimeoutSec 10)
            $medicamentos = @(Invoke-RestMethod -Uri "http://127.0.0.1:$ApiPort/api/medicamentos" -TimeoutSec 10)

            $nomesPacientes = @($pacientes | ForEach-Object { $_.nomeCompleto })
            $nomesMedicamentos = @($medicamentos | ForEach-Object { $_.nome })

            $esperadosPacientes = @('Paciente Exemplo Um', 'Paciente Exemplo Dois')
            $esperadosMedicamentos = @('Medicamento Exemplo A', 'Medicamento Exemplo B')

            if ($nomesPacientes.Count -ne 2 -or $nomesMedicamentos.Count -ne 2) {
                throw "Quantidades incorretas: pacientes=$($nomesPacientes.Count); medicamentos=$($nomesMedicamentos.Count)"
            }
            foreach ($nome in $esperadosPacientes) {
                if (@($nomesPacientes | Where-Object { $_ -ceq $nome }).Count -ne 1) {
                    throw "Paciente ausente ou duplicado: $nome"
                }
            }
            foreach ($nome in $esperadosMedicamentos) {
                if (@($nomesMedicamentos | Where-Object { $_ -ceq $nome }).Count -ne 1) {
                    throw "Medicamento ausente ou duplicado: $nome"
                }
            }

            Write-Host "OK: 2 pacientes e 2 medicamentos esperados, sem duplicatas." -ForegroundColor Green
            return
        }
        catch {
            if ($attempt -eq $MaxTentativas) {
                throw "Catalogos nao validados apos $MaxTentativas tentativa(s). $($_.Exception.Message)"
            }
            Start-Sleep -Seconds 3
        }
    }
}

$started = $false
try {
    # Valores isolados, nao gravados em .env nem no repositorio.
    $env:MYSQL_ROOT_PASSWORD = 'EvalRoot-' + [Guid]::NewGuid().ToString('N')
    $env:MYSQL_PASSWORD = 'EvalUser-' + [Guid]::NewGuid().ToString('N')
    $env:MYSQL_DATABASE = 'medical_prescription'
    $env:MYSQL_USER = 'medical_user'
    $env:MYSQL_PORT = [string]$MySqlPort
    $env:API_PORT = [string]$ApiPort
    $env:FRONTEND_PORT = [string]$FrontendPort

    Write-Host "Projeto Compose temporario: $project" -ForegroundColor Cyan
    Write-Host 'Validando configuracao e construindo imagens...' -ForegroundColor Cyan
    Invoke-Compose -Arguments @('config', '--quiet')
    $started = $true
    Invoke-Compose -Arguments @('up', '-d', '--build')
    Invoke-Compose -Arguments @('ps', '-a')

    Write-Host 'Conferindo seed no primeiro boot...' -ForegroundColor Cyan
    Test-Catalogos -MaxTentativas 60

    Write-Host 'Reiniciando API para verificar idempotencia...' -ForegroundColor Cyan
    Invoke-Compose -Arguments @('restart', 'api')
    Test-Catalogos -MaxTentativas 60

    Write-Host 'PRIMEIRA INSTALACAO E SEED VALIDADOS COM SUCESSO!' -ForegroundColor Green
    Write-Host 'O volume padrao do projeto nao foi utilizado.' -ForegroundColor Green
}
catch {
    Write-Host '--- Diagnostico de containers ---' -ForegroundColor Yellow
    if ($started) {
        & docker compose -p $project -f $compose ps -a
        & docker compose -p $project -f $compose logs --tail=80 api migrate mysql
    }
    throw
}
finally {
    if ($started -and -not $KeepEnvironment) {
        Write-Host 'Removendo apenas o projeto temporario e seus volumes...' -ForegroundColor Cyan
        & docker compose -p $project -f $compose down --volumes --remove-orphans
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Nao foi possivel limpar $project. Confira com docker compose -p $project ps -a."
        }
    }
    elseif ($started) {
        Write-Host "Ambiente temporario preservado: $project" -ForegroundColor Yellow
        Write-Host "Para remover depois, execute o comando compose down -v com -p $project." -ForegroundColor Yellow
    }
    foreach ($key in $keys) {
        [Environment]::SetEnvironmentVariable($key, $original[$key], 'Process')
    }
}
