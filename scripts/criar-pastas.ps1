
$ErrorActionPreference = "Stop"

$raiz = "C:\Projetos\prescricao-medica"
$base = Join-Path $raiz "backend\MedicalPrescription"
$utf8 = New-Object System.Text.UTF8Encoding($false)

$domain = Join-Path $base `
    "MedicalPrescription.Domain\Entities\Prescricao.cs"

$teste = Join-Path $base `
    "MedicalPrescription.UnitTests\Entities\PrescricaoCriacaoAtomicaTests.cs"

$arquivoBak = Join-Path $base "MedicalPrescription.slnx.bak"
$solucao = Join-Path $base "MedicalPrescription.slnx"

$unitTests = Join-Path $base `
    "MedicalPrescription.UnitTests\MedicalPrescription.UnitTests.csproj"

$integrationTests = Join-Path $base `
    "MedicalPrescription.IntegrationTests\MedicalPrescription.IntegrationTests.csproj"

Set-Location $raiz

if ((git branch --show-current).Trim() -ne "refactor/domain-final") {
    throw "Execute na branch refactor/domain-final."
}

foreach ($arquivo in @($domain, $solucao, $unitTests, $integrationTests)) {
    if (-not (Test-Path $arquivo)) {
        throw "Arquivo nao encontrado: $arquivo"
    }
}

$relDomain = "backend/MedicalPrescription/MedicalPrescription.Domain/Entities/Prescricao.cs"

if (@(git status --porcelain -- $relDomain).Count -gt 0) {
    throw "Prescricao.cs possui alteracoes locais. Revise antes de executar."
}

$backup = Join-Path $env:TEMP (
    "MedicalPrescription-DomainFinal-" +
    (Get-Date -Format "yyyyMMddHHmmssfff")
)

New-Item -ItemType Directory -Force -Path $backup | Out-Null

$arquivos = @($domain, $teste, $arquivoBak)
$originais = @{}

foreach ($arquivo in $arquivos) {
    if (Test-Path $arquivo) {
        $destino = Join-Path $backup ([IO.Path]::GetFileName($arquivo))
        Copy-Item $arquivo $destino -Force
        $originais[$arquivo] = $destino
    }
}

try {
    # ========================================
    # 1. CORRIGIR A CRIACAO DA PRESCRICAO
    # ========================================

    $conteudo = [IO.File]::ReadAllText($domain)

    $inicioMarcador = "    public static Prescricao CriarEstruturada("
    $fimMarcador = "    public void AtualizarTextoLivre("

    $inicio = $conteudo.IndexOf(
        $inicioMarcador,
        [StringComparison]::Ordinal
    )

    $fim = $conteudo.IndexOf(
        $fimMarcador,
        [StringComparison]::Ordinal
    )

    if ($inicio -lt 0 -or $fim -le $inicio) {
        throw "Estrutura de Prescricao.cs diferente da esperada."
    }

    $novoMetodo = @'
    public static Prescricao CriarEstruturada(
        Guid pacienteId,
        IEnumerable<PrescricaoItem> itens)
    {
        ArgumentNullException.ThrowIfNull(itens);

        if (pacienteId == Guid.Empty)
            throw new ArgumentException(
                "Paciente inválido.", nameof(pacienteId));

        var novosItens = itens.ToList();

        if (novosItens.Count == 0)
            throw new ArgumentException(
                "A prescrição deve conter pelo menos um medicamento.",
                nameof(itens));

        if (novosItens.Any(item => item is null))
            throw new ArgumentException(
                "A lista contém um item inválido.",
                nameof(itens));

        if (novosItens.Select(item => item.Id).Distinct().Count()
            != novosItens.Count)
            throw new ArgumentException(
                "A lista contém itens duplicados.",
                nameof(itens));

        if (novosItens.Any(item => item.PrescricaoId != Guid.Empty))
            throw new RegraNegocioException(
                "Um item já pertence a outra prescrição.");

        var prescricao = new Prescricao(
            pacienteId,
            TipoPrescricao.Estruturada);

        foreach (var item in novosItens)
            prescricao.AdicionarItem(item);

        return prescricao;
    }

'@

    $conteudo = $conteudo.Remove(
        $inicio,
        $fim - $inicio
    ).Insert($inicio, $novoMetodo + "`r`n")

    # Limpeza pontual de formatacao
    $conteudo = $conteudo.Replace(
        "using MedicalPrescription.Domain.Exceptions;`r`n`r`nusing",
        "using MedicalPrescription.Domain.Exceptions;`r`nusing"
    )

    $conteudo = [regex]::Replace(
        $conteudo,
        '(\r?\n    \})\r?\n(    public void RemoverItem)',
        '$1' + "`r`n`r`n" + '$2'
    )

    [IO.File]::WriteAllText($domain, $conteudo, $utf8)

    Write-Host "Validacao antecipada implementada." `
        -ForegroundColor Green

    # ========================================
    # 2. TESTES DE REGRESSAO
    # ========================================

    if (-not (Test-Path $teste)) {

        $codigoTestes = @'
using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Domain.Exceptions;
using Xunit;

namespace MedicalPrescription.UnitTests.Entities;

public sealed class PrescricaoCriacaoAtomicaTests
{
    [Fact]
    public void CriarEstruturada_ComItemDeOutraPrescricao_NaoDeveAlterarVinculos()
    {
        var vinculado = NovoItem();

        var origem = Prescricao.CriarEstruturada(
            Guid.NewGuid(), new[] { vinculado });

        var novo = NovoItem();

        Assert.Throws<RegraNegocioException>(() =>
            Prescricao.CriarEstruturada(
                Guid.NewGuid(), new[] { novo, vinculado }));

        Assert.Equal(Guid.Empty, novo.PrescricaoId);
        Assert.Equal(origem.Id, vinculado.PrescricaoId);
        Assert.Same(vinculado, Assert.Single(origem.Itens));
    }

    [Fact]
    public void CriarEstruturada_ComItemDuplicado_NaoDeveModificarVinculo()
    {
        var item = NovoItem();

        Assert.Throws<ArgumentException>(() =>
            Prescricao.CriarEstruturada(
                Guid.NewGuid(), new[] { item, item }));

        Assert.Equal(Guid.Empty, item.PrescricaoId);
    }

    [Fact]
    public void CriarEstruturada_ComItemNulo_NaoDeveModificarVinculo()
    {
        var item = NovoItem();

        Assert.Throws<ArgumentException>(() =>
            Prescricao.CriarEstruturada(
                Guid.NewGuid(),
                new PrescricaoItem[] { item, null! }));

        Assert.Equal(Guid.Empty, item.PrescricaoId);
    }

    [Fact]
    public void CriarEstruturada_ComItensValidos_DeveVincularTodos()
    {
        var primeiro = NovoItem();
        var segundo = NovoItem();

        var prescricao = Prescricao.CriarEstruturada(
            Guid.NewGuid(), new[] { primeiro, segundo });

        Assert.Equal(2, prescricao.Itens.Count);
        Assert.Equal(prescricao.Id, primeiro.PrescricaoId);
        Assert.Equal(prescricao.Id, segundo.PrescricaoId);
    }

    private static PrescricaoItem NovoItem()
    {
        return new PrescricaoItem(
            medicamentoId: null,
            nomeMedicamento: "Medicamento de teste",
            formaFarmaceutica: "Comprimido",
            dose: 250m,
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

        [IO.File]::WriteAllText($teste, $codigoTestes, $utf8)

        Write-Host "Quatro testes de regressao criados." `
            -ForegroundColor Green
    }

    # ========================================
    # 3. REMOVER BACKUP DA SOLUCAO
    # ========================================

    if (Test-Path $arquivoBak) {
        Remove-Item $arquivoBak -Force
        Write-Host "Arquivo .slnx.bak removido." `
            -ForegroundColor Green
    }

    # ========================================
    # 4. COMPILACAO
    # ========================================

    Write-Host "`nCompilando..." -ForegroundColor Cyan

    dotnet build $solucao

    if ($LASTEXITCODE -ne 0) {
        throw "Build falhou."
    }

    # ========================================
    # 5. TESTES UNITARIOS
    # ========================================

    Write-Host "`nTestes unitarios..." -ForegroundColor Cyan

    dotnet test $unitTests --no-build

    if ($LASTEXITCODE -ne 0) {
        throw "Testes unitarios falharam."
    }

    # ========================================
    # 6. TESTES DE INTEGRACAO
    # ========================================

    Write-Host "`nTestes MySQL..." -ForegroundColor Cyan

    dotnet test $integrationTests --no-build

    if ($LASTEXITCODE -ne 0) {
        throw "Testes de integracao falharam."
    }

    # ========================================
    # 7. VALIDACAO GIT
    # ========================================

    git diff --check -- backend/

    if ($LASTEXITCODE -ne 0) {
        throw "Falha na verificacao de formatacao."
    }

    Write-Host "`nDOMAIN VALIDADO COM SUCESSO!" `
        -ForegroundColor Green
}
catch {
    $motivo = $_

    foreach ($arquivo in $arquivos) {
        if ($originais.ContainsKey($arquivo)) {
            Copy-Item $originais[$arquivo] $arquivo -Force
        }
        elseif (Test-Path $arquivo) {
            Remove-Item $arquivo -Force
        }
    }

    throw "Alteracoes revertidas. Motivo: $motivo"
}

Write-Host "`nArquivos alterados:" -ForegroundColor Cyan
git status --short

Write-Host "`nBackup: $backup" -ForegroundColor Cyan
