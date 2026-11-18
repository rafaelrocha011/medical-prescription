using MedicalPrescription.Application.Abstractions.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace MedicalPrescription.Api.Controllers.OData;

public sealed class ConsultaPrescricoesController : ODataController
{
    private readonly IPrescricaoODataConsultaRepository _consulta;

    public ConsultaPrescricoesController(
        IPrescricaoODataConsultaRepository consulta)
    {
        _consulta = consulta;
    }

    [EnableQuery(
        PageSize = 50,
        MaxTop = 100,
        MaxNodeCount = 50,
        EnsureStableOrdering = true,
        AllowedQueryOptions =
            AllowedQueryOptions.Filter |
            AllowedQueryOptions.OrderBy |
            AllowedQueryOptions.Select |
            AllowedQueryOptions.Skip |
            AllowedQueryOptions.Top |
            AllowedQueryOptions.Count,
        AllowedOrderByProperties =
            "Id,PacienteId,Tipo,CriadaEmUtc,QuantidadeMedicamentos")]
    public IActionResult Get()
    {
        return Ok(_consulta.Consultar());
    }
}
