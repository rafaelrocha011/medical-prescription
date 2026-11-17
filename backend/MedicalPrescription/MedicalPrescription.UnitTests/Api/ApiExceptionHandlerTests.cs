using MedicalPrescription.Domain.Exceptions;
using System.Text.Json;
using MedicalPrescription.Api.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MedicalPrescription.UnitTests.Api;

public sealed class ApiExceptionHandlerTests
{
    [Theory]
    [InlineData("nao-encontrado", 404)]
    [InlineData("argumento", 400)]
    [InlineData("conflito", 409)]
    [InlineData("concorrencia", 409)]
    [InlineData("tecnico", 500)]
    [InlineData("inesperado", 500)]
    public async Task Excecao_DeveRetornarProblemDetails(
        string tipo,
        int statusEsperado)
    {
        using var services = new ServiceCollection()
    .AddLogging()
    .AddProblemDetails()
    .BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = services,
            TraceIdentifier = "teste-trace-123"
        };

        context.Request.Path = "/api/prescricoes";
        context.Response.Body = new MemoryStream();

        var excecao = tipo switch
        {
            "nao-encontrado" =>
                new KeyNotFoundException("Nao encontrado."),

            "argumento" =>
                new ArgumentException("Parametro invalido."),

            "conflito" =>
                new RegraNegocioException("Estado invalido."),

            "concorrencia" =>
                new DbUpdateConcurrencyException(
                    "Registro foi modificado."),

            "tecnico" =>
                new InvalidOperationException(
                    "Falha tecnica confidencial."),

            _ => new Exception("Detalhe tecnico confidencial.")
        };

        var handler = new ApiExceptionHandler();

        var tratado = await handler.TryHandleAsync(
            context,
            excecao,
            CancellationToken.None);

        Assert.True(tratado);
        Assert.Equal(statusEsperado, context.Response.StatusCode);

        Assert.StartsWith(
            "application/problem+json",
            context.Response.ContentType);

        context.Response.Body.Position = 0;

        using var documento = await JsonDocument.ParseAsync(
            context.Response.Body);

        var corpo = documento.RootElement;

        Assert.Equal(
            statusEsperado,
            corpo.GetProperty("status").GetInt32());

        Assert.Equal(
            "teste-trace-123",
            corpo.GetProperty("traceId").GetString());

        Assert.Equal(
            "/api/prescricoes",
            corpo.GetProperty("instance").GetString());

        if (statusEsperado == 500)
        {
            var detalhe = corpo.GetProperty("detail").GetString();

            Assert.Equal("Ocorreu um erro interno.", detalhe);

            Assert.DoesNotContain(
                "Detalhe tecnico confidencial",
                detalhe!);
        }
    }
}