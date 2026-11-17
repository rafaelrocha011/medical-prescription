using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedicalPrescription.IntegrationTests;

public sealed class PrescricaoDataUtcTests
    : IClassFixture<MySqlFixture>
{
    private readonly MySqlFixture _fixture;

    public PrescricaoDataUtcTests(MySqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CriadaEmUtc_DevePreservarHorarioEFusoUtc()
    {
        var paciente = new Paciente("Paciente UTC");

        var prescricao = Prescricao.CriarNaoEstruturada(
            paciente.Id,
            "Prescricao para teste de data UTC.");

        var dataOriginal = prescricao.CriadaEmUtc;

        Assert.Equal(DateTimeKind.Utc, dataOriginal.Kind);

        // Persistir usando o banco MySQL temporario.
        await using (var context = _fixture.NovoContexto())
        {
            context.Pacientes.Add(paciente);
            context.Prescricoes.Add(prescricao);

            await context.SaveChangesAsync();
        }

        // Consultar usando um DbContext diferente.
        await using (var context = _fixture.NovoContexto())
        {
            var recuperada = await context.Prescricoes
                .AsNoTracking()
                .SingleAsync(p => p.Id == prescricao.Id);

            Assert.Equal(
                DateTimeKind.Utc,
                recuperada.CriadaEmUtc.Kind);

            // MySQL datetime(6) preserva microssegundos.
            // Verificar uma tolerancia inferior a 1 ms.
            var diferenca = Math.Abs(
                (recuperada.CriadaEmUtc - dataOriginal)
                .TotalMilliseconds);

            Assert.True(diferenca < 1);
        }

        // Validar a projecao do ReadRepository.
        await using (var context = _fixture.NovoContexto())
        {
            var repository = new PrescricaoReadRepository(context);

            var detalhes = await repository.ObterPorIdAsync(
                prescricao.Id);

            Assert.NotNull(detalhes);

            Assert.Equal(
                DateTimeKind.Utc,
                detalhes.CriadaEmUtc.Kind);

            // Validar tambem a consulta utilizada pelo OData.
            var odata = await repository.Consultar()
                .SingleAsync(p => p.Id == prescricao.Id);

            Assert.Equal(
                DateTimeKind.Utc,
                odata.CriadaEmUtc.Kind);
        }
    }
}