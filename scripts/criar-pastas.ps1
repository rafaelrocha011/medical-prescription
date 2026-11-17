
$ErrorActionPreference = "Stop"

$baseUrl = "http://localhost:5080"

$consultas = @(
    @{
        Nome = "SERVICO ODATA"
        Url = $baseUrl + '/odata'
    },
    @{
        Nome = "TODAS AS PRESCRICOES"
        Url = $baseUrl + '/odata/ConsultaPrescricoes'
    },
    @{
        Nome = "FILTRAR PRESCRICOES ESTRUTURADAS"
        Url = $baseUrl + '/odata/ConsultaPrescricoes?$filter=Tipo%20eq%202'
    },
    @{
        Nome = "ORDENAR POR DATA"
        Url = $baseUrl + '/odata/ConsultaPrescricoes?$orderby=CriadaEmUtc%20desc'
    },
    @{
        Nome = "SELECIONAR CAMPOS"
        Url = $baseUrl + '/odata/ConsultaPrescricoes?$select=Id,Tipo,QuantidadeMedicamentos'
    },
    @{
        Nome = "LIMITAR RESULTADOS"
        Url = $baseUrl + '/odata/ConsultaPrescricoes?$top=10'
    },
    @{
        Nome = "CONTAR REGISTROS"
        Url = $baseUrl + '/odata/ConsultaPrescricoes?$count=true'
    }
)

foreach ($consulta in $consultas) {

    Write-Host ""
    Write-Host "================================" -ForegroundColor Cyan
    Write-Host $consulta.Nome -ForegroundColor Cyan
    Write-Host "================================" -ForegroundColor Cyan

    try {
        $resultado = Invoke-RestMethod `
            -Uri $consulta.Url `
            -Method Get `
            -ErrorAction Stop

        $resultado | ConvertTo-Json -Depth 10

        Write-Host "TESTE OK" -ForegroundColor Green
    }
    catch {
        Write-Host "TESTE FALHOU" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        throw
    }
}

Write-Host ""
Write-Host "TODOS OS TESTES ODATA PASSARAM!" -ForegroundColor Green
