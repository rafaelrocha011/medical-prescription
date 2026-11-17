$raiz = "C:\Projetos\prescricao-medica"

$projeto = Get-ChildItem -Path $raiz `
    -Filter "MedicalPrescription.UnitTests.csproj" `
    -Recurse -File |
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
    Select-Object -First 1

if (-not $projeto) {
    throw "Projeto de testes não encontrado."
}

$pasta = Join-Path $projeto.DirectoryName "UseCases\Prescricoes"

New-Item -ItemType Directory -Path $pasta -Force |
    Out-Null

$arquivo = Join-Path $pasta "CriarPrescricaoUseCaseTests.cs"

if (-not (Test-Path $arquivo)) {
    New-Item -ItemType File -Path $arquivo | Out-Null
}

Write-Host "Arquivo preparado: $arquivo" -ForegroundColor Green