
$ErrorActionPreference = "Stop"

$raiz = "C:\Projetos\prescricao-medica"
Set-Location $raiz

if ((git branch --show-current).Trim() -ne "refactor/backend") {
    throw "Execute na branch refactor/backend."
}

function EncontrarProjeto($nome) {
    $projetos = @(
        Get-ChildItem $raiz -Filter "$nome.csproj" -Recurse -File |
        Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
    )

    if ($projetos.Count -ne 1) {
        throw "Projeto nao encontrado de forma unica: $nome"
    }

    return $projetos[0]
}

$infra = EncontrarProjeto "MedicalPrescription.Infrastructure"
$api = EncontrarProjeto "MedicalPrescription.Api"
$tests = EncontrarProjeto "MedicalPrescription.UnitTests"

$pasta = Join-Path $infra.DirectoryName "Persistence\Repositories"

$consulta = Join-Path $pasta "PrescricaoConsultaRepository.cs"
$odata = Join-Path $pasta "PrescricaoODataConsultaRepository.cs"
$di = Join-Path $infra.DirectoryName "DependencyInjection.cs"

foreach ($arquivo in @($consulta, $di)) {
    if (-not (Test-Path $arquivo)) {
        throw "Arquivo nao encontrado: $arquivo"
    }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)

function Ler($arquivo) {
    return [System.IO.File]::ReadAllText($arquivo)
}

function Salvar($arquivo, $conteudo) {
    [System.IO.File]::WriteAllText($arquivo, $conteudo, $utf8)
}

function LimparEspacos($texto) {
    # Remove espacos e tabs no final de cada linha
    return [regex]::Replace(
        $texto,
        '(?m)[ \t]+(?=\r?$)',
        ''
    )
}

# ==========================================
# 1. BACKUP
# ==========================================

$backup = Join-Path $env:TEMP (
    "MedicalPrescription-PrescricaoRefactor-" +
    (Get-Date -Format "yyyyMMddHHmmss")
)

New-Item -ItemType Directory -Force -Path $backup | Out-Null

$originais = @{}

foreach ($arquivo in @($consulta, $odata, $di)) {
    if (Test-Path $arquivo) {
        $destino = Join-Path $backup ([IO.Path]::GetFileName($arquivo))

        Copy-Item $arquivo $destino -Force
        $originais[$arquivo] = $destino
    }
}

Write-Host "Backup: $backup" -ForegroundColor Cyan

# ==========================================
# 2. EXTRAIR METODO CONSULTAR DO ODATA
# ==========================================

function ExtrairMetodoConsultar($texto) {
    $padrao = 'public\s+IQueryable<PrescricaoODataDto>\s+Consultar\s*\(\s*\)\s*\{'

    $match = [regex]::Match($texto, $padrao)

    if (-not $match.Success) {
        throw "Metodo Consultar nao encontrado."
    }

    $inicio = $match.Index
    $abertura = $texto.IndexOf('{', $inicio)
    $nivel = 0

    for ($j = $abertura; $j -lt $texto.Length; $j++) {
        if ($texto[$j] -eq '{') {
            $nivel++
        }
        elseif ($texto[$j] -eq '}') {
            $nivel--

            if ($nivel -eq 0) {
                return $texto.Substring(
                    $inicio,
                    $j - $inicio + 1
                )
            }
        }
    }

    throw "Nao foi possivel localizar o final do metodo Consultar."
}

# ==========================================
# 3. CONSOLIDAR REPOSITORIOS
# ==========================================

try {
    if (Test-Path $odata) {

        $conteudoConsulta = Ler $consulta
        $conteudoOData = Ler $odata
        $conteudoDI = Ler $di

        $padraoClasse =
            'public\s+sealed\s+class\s+PrescricaoConsultaRepository\s*:\s*IPrescricaoConsultaRepository\b'

        if ([regex]::Matches(
            $conteudoConsulta, $padraoClasse
        ).Count -ne 1) {
            throw "Declaracao de PrescricaoConsultaRepository inesperada."
        }

        if ($conteudoConsulta -match '\bConsultar\s*\(') {
            throw "Metodo Consultar ja existe no repositorio."
        }

        # Extrair o metodo da implementacao original
        $metodo = ExtrairMetodoConsultar $conteudoOData

        # Validar os registros anteriores da DI
        $padraoDIConsulta =
            'services\s*\.\s*AddScoped\s*<\s*IPrescricaoConsultaRepository\s*,\s*PrescricaoConsultaRepository\s*>\s*\(\s*\)\s*;'

        $padraoDIOData =
            'services\s*\.\s*AddScoped\s*<\s*IPrescricaoODataConsultaRepository\s*,\s*PrescricaoODataConsultaRepository\s*>\s*\(\s*\)\s*;'

        if ([regex]::Matches(
            $conteudoDI, $padraoDIConsulta
        ).Count -ne 1) {
            throw "Registro de consulta na DI nao encontrado."
        }

        if ([regex]::Matches(
            $conteudoDI, $padraoDIOData
        ).Count -ne 1) {
            throw "Registro OData na DI nao encontrado."
        }

        # Atualizar interfaces implementadas
        $conteudoConsulta = [regex]::Replace(
            $conteudoConsulta,
            $padraoClasse,
            'public sealed class PrescricaoConsultaRepository : IPrescricaoConsultaRepository, IPrescricaoODataConsultaRepository'
        )

        # Garantir importacao do DTO
        $usingDto =
            "using MedicalPrescription.Application.DTOs.Prescricoes;"

        if (-not $conteudoConsulta.Contains($usingDto)) {
            $conteudoConsulta = $usingDto + "`r`n" + $conteudoConsulta
        }

        # Inserir o metodo Consultar antes do fechamento da classe
        $posicao = $conteudoConsulta.LastIndexOf('}')

        if ($posicao -lt 0) {
            throw "Fechamento da classe nao encontrado."
        }

        $conteudoConsulta = $conteudoConsulta.Insert(
            $posicao,
            "`r`n    $metodo`r`n"
        )

        # Corrigir formatacao antes de salvar
        $conteudoConsulta = LimparEspacos $conteudoConsulta

        Salvar $consulta $conteudoConsulta

        Write-Host "Repositorio de leitura consolidado." `
            -ForegroundColor Green

        # ==========================================
        # 4. ATUALIZAR DEPENDENCY INJECTION
        # ==========================================

        $conteudoDI = [regex]::Replace(
            $conteudoDI,
            $padraoDIConsulta,
            ''
        )

        $conteudoDI = [regex]::Replace(
            $conteudoDI,
            $padraoDIOData,
            ''
        )

        $marcador = "        return services;"

        if (-not $conteudoDI.Contains($marcador)) {
            throw "Ponto de registro na DI nao localizado."
        }

        $registros = @'
        services.AddScoped<PrescricaoConsultaRepository>();

        services.AddScoped<IPrescricaoConsultaRepository>(
            sp => sp.GetRequiredService<PrescricaoConsultaRepository>());

        services.AddScoped<IPrescricaoODataConsultaRepository>(
            sp => sp.GetRequiredService<PrescricaoConsultaRepository>());

'@

        $conteudoDI = $conteudoDI.Replace(
            $marcador,
            $registros + $marcador
        )

        # Corrigir os espacos que causaram o erro anterior
        $conteudoDI = LimparEspacos $conteudoDI

        # Reduzir excesso de linhas vazias
        $conteudoDI = [regex]::Replace(
            $conteudoDI,
            '(?:\r?\n){3,}',
            "`r`n`r`n"
        )

        Salvar $di $conteudoDI

        Write-Host "Dependency Injection atualizada." `
            -ForegroundColor Green

        # Remover somente a classe redundante
        Remove-Item -LiteralPath $odata

        Write-Host "Repositorio OData redundante removido." `
            -ForegroundColor Yellow
    }
    else {
        $conteudo = Ler $consulta

        if (
            $conteudo -notmatch 'IPrescricaoODataConsultaRepository' -or
            $conteudo -notmatch '\bConsultar\s*\('
        ) {
            throw "Repositorio OData ausente e consolidacao incompleta."
        }

        # Permite executar novamente sem duplicar implementacoes
        Write-Host "Repositorio ja consolidado." `
            -ForegroundColor Yellow
    }

    # ==========================================
    # 5. BUILD DA API
    # ==========================================

    Write-Host "`nCompilando API..." -ForegroundColor Cyan

    dotnet build $api.FullName

    if ($LASTEXITCODE -ne 0) {
        throw "Falha na compilacao."
    }

    # ==========================================
    # 6. TESTES UNITARIOS
    # ==========================================

    Write-Host "`nExecutando testes..." -ForegroundColor Cyan

    dotnet test $tests.FullName --no-restore

    if ($LASTEXITCODE -ne 0) {
        throw "Falha nos testes."
    }

    # ==========================================
    # 7. VALIDAR FORMATACAO DOS ARQUIVOS ALTERADOS
    # ==========================================

    $relConsulta = $consulta.Substring($raiz.Length + 1).Replace('\', '/')
    $relDI = $di.Substring($raiz.Length + 1).Replace('\', '/')

    git diff --check -- $relConsulta $relDI

    if ($LASTEXITCODE -ne 0) {
        throw "Formatacao invalida nos arquivos refatorados."
    }

    Write-Host "`nREFATORACAO VALIDADA COM SUCESSO!" `
        -ForegroundColor Green
}
catch {
    $motivo = $_

    foreach ($arquivo in @($consulta, $odata, $di)) {
        if ($originais.ContainsKey($arquivo)) {
            Copy-Item -LiteralPath $originais[$arquivo] `
                -Destination $arquivo -Force
        }
        elseif (Test-Path $arquivo) {
            Remove-Item -LiteralPath $arquivo -Force
        }
    }

    throw "Refatoracao revertida. Motivo: $motivo"
}

Write-Host "`nArquivos alterados:" -ForegroundColor Cyan

git status --short

Write-Host "`nBackup: $backup" -ForegroundColor Cyan
