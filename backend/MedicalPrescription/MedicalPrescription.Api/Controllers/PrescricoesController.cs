using MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;
using MedicalPrescription.Application.DTOs.Prescricoes;
using Microsoft.AspNetCore.Mvc;

namespace MedicalPrescription.Api.Controllers;

[ApiController]
[Route("api/prescricoes")]
public sealed class PrescricoesController : ControllerBase
{
    private readonly ICriarPrescricaoUseCase _criar;
    private readonly IAtualizarPrescricaoUseCase _atualizar;
    private readonly IObterPrescricaoUseCase _obter;
    private readonly IListarPrescricoesUseCase _listar;
    private readonly IExcluirPrescricaoUseCase _excluir;

    public PrescricoesController(
        ICriarPrescricaoUseCase criar,
        IAtualizarPrescricaoUseCase atualizar,
        IObterPrescricaoUseCase obter,
        IListarPrescricoesUseCase listar,
        IExcluirPrescricaoUseCase excluir)
    {
        _criar = criar;
        _atualizar = atualizar;
        _obter = obter;
        _listar = listar;
        _excluir = excluir;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<PrescricaoResumoDto>>> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _listar.ExecutarAsync(
            pagina, tamanhoPagina, cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("{id:guid}", Name = "ObterPrescricao")]
    public async Task<ActionResult<PrescricaoDetalheDto>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var resultado = await _obter.ExecutarAsync(
            id, cancellationToken);

        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] CriarPrescricaoRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _criar.ExecutarAsync(
            request, cancellationToken);

        return CreatedAtRoute(
            "ObterPrescricao",
            new { id },
            new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarPrescricaoRequest request,
        CancellationToken cancellationToken)
    {
        await _atualizar.ExecutarAsync(
            id, request, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _excluir.ExecutarAsync(
            id, cancellationToken);

        return NoContent();
    }
}
