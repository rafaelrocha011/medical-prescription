using MedicalPrescription.Application.Abstractions.UseCases.Catalogos;
using MedicalPrescription.Application.DTOs.Catalogos;
using Microsoft.AspNetCore.Mvc;

namespace MedicalPrescription.Api.Controllers;

[ApiController]
[Route("api/pacientes")]
public sealed class PacientesController : ControllerBase
{
    private readonly IListarPacientesUseCase _useCase;

    public PacientesController(IListarPacientesUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PacienteResumoDto>>> Listar(
        [FromQuery] string? busca,
        CancellationToken cancellationToken)
    {
        var resultado = await _useCase.ExecutarAsync(
            busca, cancellationToken);

        return Ok(resultado);
    }
}
