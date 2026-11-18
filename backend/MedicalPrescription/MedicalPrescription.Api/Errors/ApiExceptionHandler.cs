using MedicalPrescription.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.Api.Errors;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, titulo) = exception switch
        {
            KeyNotFoundException =>
                (StatusCodes.Status404NotFound,
                 "Recurso nao encontrado"),

            ArgumentException =>
                (StatusCodes.Status400BadRequest,
                 "Requisicao invalida"),

            DbUpdateConcurrencyException =>
                (StatusCodes.Status409Conflict,
                 "Conflito de concorrencia"),

            RegraNegocioException =>
                (StatusCodes.Status409Conflict,
                 "Operacao nao permitida"),

            _ =>
                (StatusCodes.Status500InternalServerError,
                 "Erro interno")
        };

        var detalhe = status switch
        {
            StatusCodes.Status500InternalServerError =>
                "Ocorreu um erro interno.",

            StatusCodes.Status409Conflict =>
                "A operacao nao pode ser concluida no estado atual.",

            _ => exception.Message
        };

        await Results.Problem(
            statusCode: status,
            title: titulo,
            detail: detalhe,
            instance: context.Request.Path.ToString(),
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = context.TraceIdentifier
            })
            .ExecuteAsync(context);

        return true;
    }
}