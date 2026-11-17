using MedicalPrescription.Application.Abstractions.UseCases.Catalogos;
using MedicalPrescription.Application.DTOs.Catalogos;
using Microsoft.AspNetCore.Mvc;

namespace MedicalPrescription.Api.Controllers;

[ApiController]
[Route("api/medicamentos")]
public sealed class MedicamentosController : ControllerBase
{
    private readonly IListarMedicamentosUseCase _useCase;

    public MedicamentosController(IListarMedicamentosUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MedicamentoResumoDto>>> Listar(
        [FromQuery] string? busca,
        CancellationToken cancellationToken)
    {
        var resultado = await _useCase.ExecutarAsync(
            busca, cancellationToken);

        return Ok(resultado);
    }
}
