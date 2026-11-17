using Microsoft.AspNetCore.Diagnostics;

namespace MedicalPrescription.Api.Errors;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, titulo) = exception switch
        {
            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Recurso nao encontrado"),

            ArgumentException =>
                (StatusCodes.Status400BadRequest, "Requisicao invalida"),

            InvalidOperationException =>
                (StatusCodes.Status409Conflict, "Operacao nao permitida"),

            _ =>
                (StatusCodes.Status500InternalServerError, "Erro interno")
        };

        if (status == 500)
            _logger.LogError(exception, "Erro nao tratado na API");
        else
            _logger.LogWarning(exception, "Requisicao rejeitada");

        var detalhe = status == 500
            ? "Ocorreu um erro interno."
            : status == 409
                ? "A operacao nao pode ser concluida no estado atual."
                : exception.Message;

        await Results.Problem(
            statusCode: status,
            title: titulo,
            detail: detalhe,
            instance: context.Request.Path.ToString())
            .ExecuteAsync(context);

        return true;
    }
}
